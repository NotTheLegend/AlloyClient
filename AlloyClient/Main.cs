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
using Alloy.UiLib.Enums;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Signals;
using Alloy.Common;
using Alloy.Common.ContentReaders;
using Alloy.Engine;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient;

public sealed class Main() : GameWindow(new Version(4, 6), Program.LogFactory) {

    public static readonly Signal OnQuit = new ();
    public static readonly Signal<ScreenType> OnScreenChange = new();
    public static readonly Signal OnFullscreenToggle = new();
    
    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(Main));
    
    public static Atlas Atlas { get; private set; }
    public static Atlas UiAtlas { get; private set; }
    
    // TODO: Would like to remove these two time
    public static double GetTime() => GameTime.TotalMs;
    
    public static GameTime GameTime { get; private set; }

    protected override void Initialize() {
        Toolkit.Window.SetTitle(Window, "RealmTk");
        Toolkit.Window.SetMinClientSize(Window, 800, 600);

        if (Settings.LastWindowMode.Value == WindowMode.Hidden) { // in case it somehow ends up hidden
            Settings.LastWindowMode.Set(WindowMode.Normal);
            Settings.ScreenWidth.ResetToDefault();
            Settings.ScreenHeight.ResetToDefault();
        }

        if (Settings.LastWindowMode.Value == WindowMode.Minimized) {
            Settings.LastWindowMode.Set(WindowMode.Normal);
        }
        
        
        Toolkit.Window.SetSize(Window, new Vector2i(Settings.ScreenWidth, Settings.ScreenHeight));
        Toolkit.Window.SetPosition(Window, new Vector2i(Settings.LastWindowPositionX, Settings.LastWindowPositionY));
        Toolkit.Window.SetMode(Window, Settings.LastWindowMode);
        GL.Viewport(0, 0, Settings.ScreenWidth, Settings.ScreenHeight);

        OnQuit.Add(Exit);
        OnScreenChange.Add(SetGraphicOptions);
        OnFullscreenToggle.Add(ToggleFullscreen);
        
        
        Audio.Init(Program.LogFactory, Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, @"Content\Sound"));
        
        GL.ClearColor(0f, 0f, 0f, 1.0f);
        GL.Disable(EnableCap.StencilTest);
        GL.CullFace(TriangleFace.Front);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.FramebufferSrgb);
        
        var settings = new UiSettings {
            DefaultScreen = new Vector2i(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight),
            Screen = new Vector2i(Settings.ScreenWidth, Settings.ScreenHeight)
        };
        
        ContentReader.Init(Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, "Content"));
        UiRender.ConfigureAndLoad(Program.LogFactory, settings, out var stage);
        DisplayManager.Init(stage);
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH")]
    protected override void LoadContent() {
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

        
        
        
        UiRender.RegisterFont(font);
        UiRender.RegisterTexture(TextureType.GameAtlas, gameAtlasSampler);
        UiRender.RegisterTexture(TextureType.UiAtlas, uiAtlasSampler);
        UiRender.RegisterTexture(TextureType.UiAtlasLinear, uiAtlasLinear);
        UiRender.RegisterTexture(TextureType.Minimap, mapTextureSampler);
        UiRender.RegisterTexture(TextureType.TitleBackground, titleBackgroundSampler);
        UiRender.RegisterTexture(TextureType.TitleGraphic, titleGraphicSampler);
        
        Render.FirstTimeInit(gameAtlasSampler);

        SliceLibrary.Load();
        
        
        
        Audio.Start();
        Audio.SetMasterVolume(Settings.GetMasterVolume());
        Audio.MusicChannel.SetVolume(Settings.GetMusicVolume());
        Audio.SfxChannel.SetVolume(Settings.GetSfxVolume());
        Audio.MusicChannel.FadeTo("Music/sorc.ogg", 2f);
        
        ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
    }

    protected override void Update(GameTime gameTime) => DisplayManager.Update(GameTime = gameTime);

    protected override void Draw(GameTime gameTime) => DisplayManager.Draw(gameTime);
    
    protected override void Stop() => Audio.Stop();

    protected override void HandleEvents(EventArgs args) {
        switch (args) {
            case CloseEventArgs:
                Exit();
                break;
            case WindowResizeEventArgs when Initialized:
                Settings.LastWindowMode.Set(Toolkit.Window.GetMode(Window));
                break;
            case WindowMoveEventArgs e when Initialized:
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