using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using AlloyClient.Assets;
using Alloy.Engine.Graphics;
using AlloyClient.Display;
using AlloyClient.Game.Components;
using AlloyClient.Rendering;
using AlloyClient.Screens;
using AlloyClient.State;
using AlloyClient.Ui;
using Alloy.UiLib;
using Alloy.UiLib.Data;
using Alloy.UiLib.Enums;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Signals;
using Alloy.Common;
using Alloy.Common.ContentReaders;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient;

public class Main {
    
    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(Main));

    public static readonly SingleSignal<GraphicsOptions> GraphicsMode = new();

    public static Main GameInstance { get; private set; }
    public static Atlas Atlas { get; private set; }
    public static Atlas UiAtlas { get; private set; }

    public readonly WindowHandle Window;
    public readonly OpenGLContextHandle Context;

    private double _targetFrameTime;

    public Main() {
        GameInstance = this;

        var options = new ToolkitOptions {
            Logger = null
        };
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

        Toolkit.Event.EventRaised += HandleEvents;
        
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
                var res = Toolkit.Display.GetResolution(display);
                Toolkit.Window.SetClientPosition(Window, new Vector2i(res.X / 2 - Settings.ScreenWidth / 2, res.Y / 2 - Settings.ScreenHeight / 2));
                break;
        }
        
        GraphicsMode.Set(SetGraphicOptions);
        Audio.Init(Program.LogFactory, Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, @"Content\Sound"));
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH")]
    private void LoadContent() {
        ContentReader.Init(Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, "Content"));
        
        Atlas = ContentReader.LoadAtlas("Game.atlas");
        UiAtlas = ContentReader.LoadAtlas("Ui.atlas");
        MinimapTexture.Init(out var mapTexture);
        var titleBackground = ContentReader.LoadTexture("TitleScreen/TitleScreenBackground.png");
        var titleGraphic = ContentReader.LoadTexture("TitleScreen/TitleScreenGraphic.png");
        var font = new BitmapFamily(ContentReader.LoadFont("Fonts/MyriadPro/MyriadPro.msdf"));
        
        // Set texture units
        var gameAtlasSampler = new Sampler(Atlas.Texture, 0);
        var uiAtlasSampler = new Sampler(UiAtlas.Texture, 1);
        var uiAtlasLinear = new Sampler(UiAtlas.Texture, TextureFilter.Linear, 2);
        var mapTextureSampler = new Sampler(mapTexture, 3);
        var titleBackgroundSampler = new Sampler(titleBackground, 4);
        var titleGraphicSampler = new Sampler(titleGraphic, 5);
        font.Sampler.Bind(6);
        
        ModelData.Load();

        var settings = new UiSettings {
            DefaultScreen = new Vector2i(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight),
            Screen = new Vector2i(Settings.ScreenWidth, Settings.ScreenHeight)
        };
        
        

        //UiRender needs to be loaded first so Render can pull font data from it
        UiRender.ConfigureAndLoad(Program.LogFactory, settings, out var stage);
        UiRender.RegisterFont(font);
        UiRender.RegisterTexture(TextureType.GameAtlas, gameAtlasSampler);
        UiRender.RegisterTexture(TextureType.UiAtlas, uiAtlasSampler);
        UiRender.RegisterTexture(TextureType.UiAtlasLinear, uiAtlasLinear);
        UiRender.RegisterTexture(TextureType.Minimap, mapTextureSampler);
        UiRender.RegisterTexture(TextureType.TitleBackground, titleBackgroundSampler);
        UiRender.RegisterTexture(TextureType.TitleGraphic, titleGraphicSampler);
        
        Render.FirstTimeInit(gameAtlasSampler);

        SliceLibrary.Load();
        
        DisplayManager.Init(stage);
        
        Audio.Start();
        Audio.SetMasterVolume(Settings.GetMasterVolume());
        Audio.MusicChannel.SetVolume(Settings.GetMusicVolume());
        Audio.SfxChannel.SetVolume(Settings.GetSfxVolume());
        Audio.MusicChannel.FadeTo("Music/sorc.ogg", 2f);

        ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
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
        
        GL.Disable(EnableCap.FramebufferSrgb);
        
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
            
            var timeToNextUpdate = (_targetFrameTime - elapsedMs) / 1000d;
            if (timeToNextUpdate > 0) OpenTK.Core.Utils.AccurateSleep(timeToNextUpdate, 1);
        }
    }
    
    private void HandleEvents(EventArgs args) {
        switch (args) {
            case CloseEventArgs:
                Exit();
                break;
            case FocusEventArgs e:
                UserInput.SetWindowFocus(e.GotFocus);
                break;
        }
    }

    public void Exit() {
        Toolkit.Window.Destroy(Window);
        _running = false;
        Audio.Stop();
    }

    public static double GetTime() => GameTime.TotalMs;
    
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
        Logger.Log(LogLevel.Warning, "[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
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