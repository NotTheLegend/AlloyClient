using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Common;
using Common.ContentReaders;
using RealmClient.UiLib;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Extra;
using RealmClient.UiLib.Signals;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using RealmClient.Assets;
using RealmClient.Display;
using RealmClient.Game.Components;
using RealmClient.Rendering;
using RealmClient.Screens;
using RealmClient.State;
using RealmClient.Ui;
using RealmClient.Utils;

namespace RealmClient;

public class Main {
    private static readonly Logger Log = new(typeof(Main));

    public static readonly SingleSignal<GraphicsOptions> GraphicsMode = new();

    public static Main GameInstance;
    public static Atlas Atlas;
    public static Atlas UiAtlas;

    public readonly WindowHandle Window;
    public readonly OpenGLContextHandle Context;

    private double _targetFrameTime;

    public Main() {
        GameInstance = this;
        
        var options = new ToolkitOptions();
        Toolkit.Init(options);
        
        var version = new Version(4, 6);
        if (!MinimumVersionCheck(version))
            return;

        var hints = new OpenGLGraphicsApiHints {
            Version = version
        };
        Window = Toolkit.Window.Create(hints);
        Context = Toolkit.OpenGL.CreateFromWindow(Window);
        
        Toolkit.OpenGL.SetCurrentContext(Context);
        GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(Context));

        EnableDebugOutput();

        EventQueue.EventRaised += HandleEvents;
        
        Initialize();
        LoadContent();
    }

    private void Initialize() {
        GL.Viewport(0, 0, Settings.ScreenWidth, Settings.ScreenHeight);
        Toolkit.Window.SetClientSize(Window, new Vector2i(Settings.ScreenWidth, Settings.ScreenHeight));
        Toolkit.Window.SetTitle(Window, "RealmTk");
        Toolkit.Window.SetMinClientSize(Window, 800, 600);

        var mode = Settings.Fullscreen ? WindowMode.WindowedFullscreen : WindowMode.Normal;
        Toolkit.Window.SetMode(Window, mode);

        switch (mode) {
            case WindowMode.WindowedFullscreen:
                Toolkit.Window.SetPosition(Window, Vector2i.Zero);
                break;
            default:
                var display = Toolkit.Window.GetDisplay(Window);
                Toolkit.Display.GetResolution(display, out var width, out var height);
                Toolkit.Window.SetClientPosition(Window, new Vector2i(width / 2 - Settings.ScreenWidth / 2, height / 2 - Settings.ScreenHeight / 2));
                break;
        }
        
        GraphicsMode.Set(SetGraphicOptions);
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH")]
    private void LoadContent() {
        ContentReader.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"));
        Atlas = ContentReader.LoadAtlas("Game.atlas", TextureFilter.Linear);
        UiAtlas = ContentReader.LoadAtlas("Ui.atlas", TextureFilter.Nearest);
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

    private void SetGraphicOptions(GraphicsOptions mode) {
        switch (mode) {
            case GraphicsOptions.TitleScreen:
                _targetFrameTime = 1000d / 60;
                break;
            case GraphicsOptions.InGame when Settings.VSync:
                Toolkit.OpenGL.SetSwapInterval(1);
                break;
            case GraphicsOptions.InGame when Settings.FpsCap > 0:
                Toolkit.OpenGL.SetSwapInterval(0);
                _targetFrameTime = 1000d / Settings.FpsCap.Value;
                break;
            case GraphicsOptions.InGame:
                _targetFrameTime = 0;
                Toolkit.OpenGL.SetSwapInterval(0);
                break;
            default:
                throw new Exception();
            
        }
    }
    
    private bool _running = true;

    public void Run() {
        if (Window == null || Context == null) return;
        
        var sw = Stopwatch.StartNew();
        var totalMs = 0d;
        
        GL.ClearColor(0f, 0f, 0f, 1.0f);
        
        GL.Disable(EnableCap.StencilTest);
        GL.CullFace(TriangleFace.Front);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        while (true) {
            var elapsedMs = sw.Elapsed.TotalMilliseconds;
            
            if (elapsedMs < _targetFrameTime) continue;
            
            sw.Restart();
            
            Toolkit.Window.ProcessEvents(false);
            
            if (!_running) break;

            totalMs += elapsedMs;

            GameTime = new GameTime(totalMs, elapsedMs);
            
            Update(new GameTime(totalMs, elapsedMs));
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            Draw(new GameTime(totalMs, elapsedMs));
            
            Toolkit.OpenGL.SwapBuffers(Context);
            
            var timeToNextUpdate = _targetFrameTime - elapsedMs * 1000d;
            if (timeToNextUpdate > 0) OpenTK.Core.Utils.AccurateSleep(0, 1);
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
        }
    }

    public void Exit() {
        Toolkit.Window.Destroy(Window);
        _running = false;
    }
    
    public static GameTime GameTime { get; private set; }
    
    [Conditional("DEBUG")]
    private static void EnableDebugOutput() {
        GL.DebugMessageCallback(OnDebugMessage, nint.Zero);
        GL.DebugMessageControl(DebugSource.DebugSourceApi, DebugType.DebugTypeOther, DebugSeverity.DontCare, 1, [131185], false);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
    }
    
    private static void OnDebugMessage(DebugSource source, DebugType type, uint id, DebugSeverity severity, int length, nint pmessage, nint userParam) {
        var message = Marshal.PtrToStringAnsi(pmessage, length);
        Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
    }

    private static bool MinimumVersionCheck(Version version) {
        var hints = new OpenGLGraphicsApiHints {
            Version = version
        };
        
        var window = Toolkit.Window.Create(hints);
        OpenGLContextHandle context = null;
        var pass = true;

        try {
            context = Toolkit.OpenGL.CreateFromWindow(window);
        } catch {
            Toolkit.Dialog.ShowMessageBox(window, "OpenGL creation failure", $"Client requires a minimum opengl version of {version.Major}.{version.Minor}", MessageBoxType.Information);
            pass = false;
        } finally {
            if (pass) {
                Toolkit.OpenGL.DestroyContext(context!);
            }
            Toolkit.Window.Destroy(window);
        }

        return pass;
    }
}