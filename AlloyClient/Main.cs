using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AlloyClient.Assets;
using Alloy.Engine.Graphics;
using AlloyClient.Display;
using AlloyClient.Game.Components;
using AlloyClient.Rendering;
using AlloyClient.Screens;
using AlloyClient.Ui;
using Alloy.UiLib;
using Alloy.UiLib.Data;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Signals;
using Alloy.Common;
using Alloy.ContentReader;
using Alloy.Engine;
using Alloy.UiLib.Core;
using AlloyClient.Game;
using AlloyClient.Logging;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient;

public sealed class Main() : GameWindow(new Version(4, 6), ILogger.Factory) {

    public static readonly Signal OnQuit = new ();
    public static readonly Signal<ScreenType> OnScreenChange = new();
    public static readonly Signal OnFullscreenToggle = new();
    
    private static readonly ILogger Logger = ILogger.CreateLogger(nameof(Main));
    
    public static Atlas Atlas { get; private set; }
    public static Atlas UiAtlas { get; private set; }
    
    // TODO: Would like to remove these two time
    public static double GetTime() => GameTime.TotalMs;
    
    public static GameTime GameTime { get; private set; }

    protected override void Initialize() {
        #region Set Initial Window State
        if (Settings.LastWindowMode.Value is WindowMode.Minimized or WindowMode.Hidden) {
            Settings.LastWindowMode.Set(WindowMode.Normal);
        }

        if (Settings.LastWindowMode == WindowMode.Normal) {
            var displayArea = Toolkit.Display.GetWorkArea(Toolkit.Window.GetDisplay(Window));
            Settings.LastWindowWidth.Set(Math.Max(Math.Min(Settings.LastWindowWidth, displayArea.Width), 800));
            Settings.LastWindowHeight.Set(Math.Max(Math.Min(Settings.LastWindowHeight, displayArea.Height), 600));
            Settings.LastWindowPositionX.Set(Math.Min(Math.Max(Settings.LastWindowPositionX, displayArea.Min.X), displayArea.Max.X - Settings.LastWindowWidth));
            Settings.LastWindowPositionY.Set(Math.Min(Math.Max(Settings.LastWindowPositionY, displayArea.Min.Y), displayArea.Max.Y - Settings.LastWindowHeight));
        }
        
        Toolkit.Window.SetPosition(Window, new Vector2i(Settings.LastWindowPositionX, Settings.LastWindowPositionY));
        Toolkit.Window.SetSize(Window, new Vector2i(Settings.LastWindowWidth, Settings.LastWindowHeight));
        Toolkit.Window.SetMode(Window, Settings.LastWindowMode);
        Toolkit.Window.SetMinClientSize(Window, 800, 600); // <-- Must be set after window state is loaded from settings
        #endregion
        Toolkit.Window.SetTitle(Window, "RealmTk");
            
        // Initial GL state
        Toolkit.Window.GetClientSize(Window, out var size);
        GL.Viewport(0, 0, size.X, size.Y);
        GL.ClearColor(0f, 0f, 0f, 1.0f);
        GL.Disable(EnableCap.StencilTest);
        GL.CullFace(TriangleFace.Front);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.FramebufferSrgb);
        
        var settings = new UiSettings {
            DefaultScreen = new Vector2i(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight),
            Screen = Settings.ScreenSize
        };
        
        // Initializers
        Audio.Init(ILogger.Factory, Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, @"Content\Sound"));
        ContentLoader.Init(Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, "Content"));
        UiRender.ConfigureAndLoad(ILogger.Factory, settings, out var stage);
        DisplayManager.Init(stage);
        
        // Audio Setup
        Audio.Start();
        Audio.SetMasterVolume(Settings.GetMasterVolume());
        Audio.MusicChannel.SetVolume(Settings.GetMusicVolume());
        Audio.SfxChannel.SetVolume(Settings.GetSfxVolume());
        
        // Signals
        OnQuit.Add(Exit);
        OnScreenChange.Add(SetGraphicOptions);
        OnFullscreenToggle.Add(ToggleFullscreen);
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH")]
    protected override void LoadContent() {
        // Load content/textures
        Atlas = ContentLoader.LoadAtlas("Game.atlas");
        UiAtlas = ContentLoader.LoadAtlas("Ui.atlas");
        MinimapTexture.Init(out var mapTexture);
        var titleBackground = ContentLoader.LoadTexture("TitleScreen/TitleScreenBackground.png");
        var titleGraphic = ContentLoader.LoadTexture("TitleScreen/TitleScreenGraphic.png");
        var font = new BitmapFamily(ContentLoader.LoadFont("Fonts/MyriadPro/MyriadPro.msdf"));
        
        // Init content that depends on any atlas
        ModelData.Load();
        SliceLibrary.Load();
        ConditionEffects.Init();
        
        // Set texture units
        var gameAtlasSampler = new Sampler(Atlas.Texture, 0);
        var uiAtlasSampler = new Sampler(UiAtlas.Texture, 1);
        var uiAtlasLinear = new Sampler(UiAtlas.Texture, TextureFilter.Linear, 2);
        var mapTextureSampler = new Sampler(mapTexture, 3);
        var titleBackgroundSampler = new Sampler(titleBackground, 4);
        var titleGraphicSampler = new Sampler(titleGraphic, 5);
        font.Sampler.Bind(6);
        
        // Render setup
        Render.FirstTimeInit(gameAtlasSampler, font);
        UiRender.RegisterFont(font);
        UiRender.RegisterTexture(TextureType.GameAtlas, gameAtlasSampler);
        UiRender.RegisterTexture(TextureType.UiAtlas, uiAtlasSampler);
        UiRender.RegisterTexture(TextureType.UiAtlasLinear, uiAtlasLinear);
        UiRender.RegisterTexture(TextureType.Minimap, mapTextureSampler);
        UiRender.RegisterTexture(TextureType.TitleBackground, titleBackgroundSampler);
        UiRender.RegisterTexture(TextureType.TitleGraphic, titleGraphicSampler);
        
        Audio.MusicChannel.FadeTo("Music/sorc.ogg", 2f);
        
        //ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
        //ScreenManager.SetScreen(new TestScreen());
        ScreenManager.SetScreen(new LoadingScreen());
    }

    protected override void Update(GameTime gameTime) => DisplayManager.Update(GameTime = gameTime);

    protected override void Draw(GameTime gameTime) => DisplayManager.Draw(gameTime);
    
    protected override void Stop() => Audio.Stop();

    protected override void HandleEvents(EventArgs args) {
        switch (args) {
            case CloseEventArgs:
                Exit();
                break;
            case WindowResizeEventArgs e:
                GL.Viewport(0, 0, e.NewClientSize.X, e.NewClientSize.Y);
                var mode = Toolkit.Window.GetMode(Window);
                if (mode != WindowMode.Hidden) {
                    Settings.LastWindowMode.Set(mode);
                }
                Settings.ScreenSize = e.NewClientSize;
                break;
            case WindowMoveEventArgs e:
                Settings.LastWindowPositionX.Set(e.WindowPosition.X);
                Settings.LastWindowPositionY.Set(e.WindowPosition.Y);
                break;
            case FocusEventArgs e:
                UserInput.SetWindowFocus(e.GotFocus);
                break;
        }
    }
    
    private void SetGraphicOptions(ScreenType mode) {
        switch (mode) {
            case ScreenType.Menu:
                TargetFrameTime = 1000d / 60;
                break;
            case ScreenType.Game when Settings.VSync:
                Toolkit.OpenGL.SetSwapInterval(1);
                TargetFrameTime = 0; // the monitor controls the speed here, so it shouldn't need to be slowed
                break;
            case ScreenType.Game when Settings.FpsCap > 0:
                Toolkit.OpenGL.SetSwapInterval(0);
                TargetFrameTime = 1000d / Settings.FpsCap.Value;
                break;
            case ScreenType.Game:
                TargetFrameTime = 0;
                Toolkit.OpenGL.SetSwapInterval(0);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private void ToggleFullscreen() {
        if (Settings.FullscreenState.Value) {
            var mode = Settings.FullscreenMode.Value switch {
                FullscreenType.Exclusive => WindowMode.ExclusiveFullscreen,
                FullscreenType.Borderless => WindowMode.WindowedFullscreen,
                _ => throw new ArgumentOutOfRangeException()
            };
                
            Toolkit.Window.SetMode(Window, mode);
        } else {
            Toolkit.Window.SetMode(Window, WindowMode.Maximized);
        }
    }
}