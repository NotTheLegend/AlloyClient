using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.ContentReaders;
using Common.Vector;

using MonoClient.Assets;
using MonoClient.Display;
using MonoClient.Rendering;
using MonoClient.Screens;
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
        var options = new ToolkitOptions();
        Toolkit.Init(options);
        
        var hints = new OpenGLGraphicsApiHints();
        Window = Toolkit.Window.Create(hints);
        Context = Toolkit.OpenGL.CreateFromWindow(Window);
        
        Toolkit.OpenGL.SetCurrentContext(Context);
        GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(Context));

        EventQueue.EventRaised += HandleEvents;
        
        //vsync control, 0 = off, 1 = on
        Toolkit.OpenGL.SetSwapInterval(0);
        
        
        Initialize();
        LoadContent();
        Run();
    }

    private void Initialize() {
        Toolkit.Window.SetClientSize(Window, new Vector2i(Settings.ScreenWidth, Settings.ScreenHeight));
        Toolkit.Window.SetTitle(Window, "RealmTk");

        var mode = Settings.Fullscreen ? WindowMode.WindowedFullscreen : WindowMode.Normal;
        Toolkit.Window.SetMode(Window, mode);
        
        GL.ClearColor(0f, 0f, 0f, 1.0f);
        

        ContentReader.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"));
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: Microsoft.Xna.Framework.Color[]")]
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.Byte[]; size: 65MB")]
    private void LoadContent() {
        Atlas = ContentReader.LoadAtlas("Game.atlas");
        UiAtlas = ContentReader.LoadAtlas("Ui.atlas");
        MinimapTexture.Init(GraphicsDevice, out var mapTexture);
        
        ModelData.Load();

        var settings = new UiSettings {
            Game = this,
            MinimumScreen = new IntVector2(800, 600),
            DefaultScreen = new IntVector2(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight)
        };

        //UiRender needs to be loaded first so Render can pull font data from it
        UiRender.ConfigureAndLoad(settings, out var stage);
        
        UiRender.RegisterFont(ContentReader.LoadFont("Fonts/MyriadPro/MyriadPro.msdf"));
        
        UiRender.RegisterTexture(TextureType.GameAtlas, Atlas.GetTexture());
        UiRender.RegisterTexture(TextureType.UiAtlas, UiAtlas.GetTexture());
        UiRender.RegisterTexture(TextureType.Minimap, mapTexture);
        UiRender.RegisterTexture(TextureType.TitleBackground, ContentReader.LoadTexture("TitleScreen/TitleScreenBackground.png"));
        UiRender.RegisterTexture(TextureType.TitleGraphic, ContentReader.LoadTexture("TitleScreen/TitleScreenGraphic.png"));
        
        Render.FirstTimeInit();
        
        SliceLibrary.Load();
        
        DisplayManager.Init(stage);

        ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
    }

    private void Update(GameTime gameTime) {
        DisplayManager.Update(gameTime);
    }

    private void Draw(GameTime gameTime) {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DisplayManager.Draw(gameTime);
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
        
        while (true) {
            var elapsedMs = sw.Elapsed.TotalMilliseconds;
            
            //TODO: replace 0 with framerate settings
            if (elapsedMs > 0) continue;
            
            Toolkit.Window.ProcessEvents(false);
            
            if (!_running) break;

            totalMs += elapsedMs;
            
            
            Update(new GameTime(totalMs, elapsedMs));
            Draw(new GameTime(totalMs, elapsedMs));

            OpenTK.Core.Utils.AccurateSleep(0, 1);
        }
    }
    
    private void HandleEvents(PalHandle handle, PlatformEventType type, EventArgs args) {
        switch (args) {
            case CloseEventArgs:
                Toolkit.Window.Destroy(Window);
                _running = false;
                break;
        }
    }
}