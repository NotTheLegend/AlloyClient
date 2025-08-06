using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Common;
using Common.ContentReaders;

using MonoClient.Assets;
using MonoClient.Display;
using MonoClient.Rendering;
using MonoClient.Screens;
using MonoClient.Screens.Game.Components;
using MonoClient.State;
using MonoClient.Ui;
using MonoClient.UiLib;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Extra;
using MonoClient.UiLib.Signals;
using MonoClient.Utils;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace MonoClient;

public class Main {
    private static readonly Logger Log = new(typeof(Main));

    public static readonly SingleSignal<GraphicsOptions> GraphicsMode = new();

    public static Main GameInstance;
    public static Atlas Atlas;
    public static Atlas UiAtlas;

    public readonly WindowHandle Window;
    public readonly OpenGLContextHandle Context;

    public Main() {
        GameInstance = this;
        
        var options = new ToolkitOptions();
        Toolkit.Init(options);
        
        var hints = new OpenGLGraphicsApiHints();
        Window = Toolkit.Window.Create(hints);
        Context = Toolkit.OpenGL.CreateFromWindow(Window);
        
        Toolkit.OpenGL.SetCurrentContext(Context);
        GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(Context));
        
        GLDebugProc debugMessageDelegate = OnDebugMessage;
        GL.DebugMessageCallback(debugMessageDelegate, nint.Zero);
        GL.DebugMessageControl(DebugSource.DebugSourceApi, DebugType.DebugTypeOther, DebugSeverity.DontCare, 1, [131185], false);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);

        EventQueue.EventRaised += HandleEvents;
        
        //vsync control, 0 = off, 1 = on
        Toolkit.OpenGL.SetSwapInterval(0);
        
        Initialize();
        LoadContent();
        return;
        
        void OnDebugMessage(DebugSource source, DebugType type, uint id, DebugSeverity severity, int length, nint pmessage, nint userParam) {
            var message = Marshal.PtrToStringAnsi(pmessage, length);
            Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
        }
    }

    private void Initialize() {
        GL.Viewport(0, 0, 1280, 720);//TODO: WTF
        Toolkit.Window.SetClientSize(Window, new Vector2i(Settings.ScreenWidth, Settings.ScreenHeight));
        Toolkit.Window.SetTitle(Window, "RealmTk");
        Toolkit.Window.SetMinClientSize(Window, 800, 600);

        var mode = Settings.Fullscreen ? WindowMode.WindowedFullscreen : WindowMode.Normal;
        Toolkit.Window.SetMode(Window, WindowMode.Normal);
        
        GL.ClearColor(0f, 0f, 0f, 1.0f);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        ContentReader.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"));
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH")]
    private void LoadContent() {
        Atlas = ContentReader.LoadAtlas("Game.atlas");
        UiAtlas = ContentReader.LoadAtlas("Ui.atlas");
        MinimapTexture.Init(out var mapTexture);
        
        ModelData.Load();

        var settings = new UiSettings {
            DefaultScreen = new Vector2i(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight),
            Screen = new Vector2i(Settings.ScreenWidth, Settings.ScreenHeight)
        };

        //UiRender needs to be loaded first so Render can pull font data from it
        UiRender.ConfigureAndLoad(settings, out var stage);
        
        UiRender.RegisterFont(ContentReader.LoadFont("Fonts/MyriadPro/MyriadPro.msdf"));
        
        UiRender.RegisterTexture(TextureType.GameAtlas, Atlas.GetTexture());
        UiRender.RegisterTexture(TextureType.UiAtlas, UiAtlas.GetTexture());
        UiRender.RegisterTexture(TextureType.Minimap, mapTexture);
        UiRender.RegisterTexture(TextureType.TitleBackground, ContentReader.LoadTexture("TitleScreen/TitleScreenBackground.png", TextureFilter.Nearest));
        UiRender.RegisterTexture(TextureType.TitleGraphic, ContentReader.LoadTexture("TitleScreen/TitleScreenGraphic.png", TextureFilter.Nearest));
        
        Render.FirstTimeInit();

        SliceLibrary.Load();
        
        DisplayManager.Init(stage);
        
        

        ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
        //ScreenManager.SetScreen(new TestScreen());
    }

    private void Update(GameTime gameTime) {
        DisplayManager.Update(gameTime);
    }

    private void Draw(GameTime gameTime) {
        DisplayManager.Draw(gameTime);
    }

    public void ToggleFullScreen() {
        Settings.Fullscreen = !Settings.Fullscreen;
        var mode = Settings.Fullscreen ? WindowMode.WindowedFullscreen : WindowMode.Normal;
        Toolkit.Window.SetMode(Window, mode);
    }

    /*private void SetGraphicOptions(GraphicsOptions mode) {
        switch (mode) {
            case GraphicsOptions.TitleScreen:
                IsFixedTimeStep = true;
                TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / 60);
                InactiveSleepTime = TimeSpan.FromMilliseconds(1000f / 60);
                Graphics.SynchronizeWithVerticalRetrace = true;
                break;
            case GraphicsOptions.InGame when Settings.VSync:
                Graphics.SynchronizeWithVerticalRetrace = true;
                break;
            case GraphicsOptions.InGame when Settings.FpsCap > 0:
                IsFixedTimeStep = true;
                TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / Settings.FpsCap.Value);
                Graphics.SynchronizeWithVerticalRetrace = false;
                break;
            case GraphicsOptions.InGame:
                IsFixedTimeStep = false;
                Graphics.SynchronizeWithVerticalRetrace = false;
                break;
            default:
                throw new Exception();
            
        }
        Graphics.ApplyChanges();
    }*/
    
    private bool _running = true;

    public void Run() {
        var sw = Stopwatch.StartNew();
        var totalMs = 0d;
        
        GL.Disable(EnableCap.StencilTest);
        GL.CullFace(TriangleFace.Front);
        
        while (true) {
            var elapsedMs = sw.Elapsed.TotalMilliseconds;
            
            //TODO: replace 0 with framerate settings
            if (elapsedMs < 0) continue;
            
            sw.Restart();
            
            Toolkit.Window.ProcessEvents(false);
            
            if (!_running) break;

            totalMs += elapsedMs;
            
            Update(new GameTime(totalMs, elapsedMs));
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            Draw(new GameTime(totalMs, elapsedMs));
            
            Toolkit.OpenGL.SwapBuffers(Context);

            OpenTK.Core.Utils.AccurateSleep(0, 1);
        }
    }
    
    private void HandleEvents(PalHandle handle, PlatformEventType type, EventArgs args) {
        switch (args) {
            case CloseEventArgs:
                Toolkit.Window.Destroy(Window);
                _running = false;
                break;
            case FocusEventArgs e:
                UserInput.SetWindowFocus(e.GotFocus);
                break;
            case WindowResizeEventArgs e:
                Camera.SetViewPort(e.NewClientSize);
                GL.Viewport(0, 0, e.NewClientSize.X, e.NewClientSize.Y);
                break;
            case MouseMoveEventArgs e:
                UserInput.SetMousePosition(e.ClientPosition);
                break;
        }
    }

    public void Exit() {
        Toolkit.Window.Destroy(Window);
        _running = false;
    }
}