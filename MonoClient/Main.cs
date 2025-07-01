using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.ContentReaders;
using Common.Vector;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using MonoClient.Assets;
using MonoClient.Display;
using MonoClient.Rendering;
using MonoClient.Screens;
using MonoClient.State;
using MonoClient.Ui;
using MonoClient.UiLib;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Signals;
using MonoClient.Utils;
using Easing = MonoClient.UiLib.Extra.Easing;

namespace MonoClient;

public class Main : Game {
    private static readonly Logger Log = new(typeof(Main));

    public static GraphicsDeviceManager Graphics;

    public static readonly SingleSignal<GraphicsOptions> GraphicsMode = new();

    public static Main GameInstance;
    public static Atlas Atlas;
    public static Atlas UiAtlas;

    public Main() {
        Graphics = new GraphicsDeviceManager(this);
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        GameInstance = this;
        GraphicsMode.Set(SetGraphicOptions);
    }

    protected override void Initialize() {
        Window.Title = "Mono 7.0";

        Graphics.IsFullScreen = Settings.Fullscreen;
        Graphics.HardwareModeSwitch = !Settings.Fullscreen;
        Graphics.PreferredBackBufferWidth = Settings.ScreenWidth;
        Graphics.PreferredBackBufferHeight = Settings.ScreenHeight;
        Graphics.ApplyChanges();
        
        Window.AllowUserResizing = true;

        Map.GraphicsDevice = Graphics.GraphicsDevice;

        ContentReader.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"), GraphicsDevice);

        base.Initialize();
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: Microsoft.Xna.Framework.Color[]")]
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.Byte[]; size: 65MB")]
    protected override void LoadContent() {
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
        //ScreenManager.FadeToScreen(new TestScreen(), Easing.SineInOut, 1000, 0x0);


        /* uncomment to force restart app/world servers, takes around 1 minute for reboot to finish */
        //AppEngineClient.SendRequest("/dev/backup/restart");
    }

    protected override void Update(GameTime gameTime) {
        base.Update(gameTime);
        DisplayManager.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
        //GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        DisplayManager.Draw(gameTime);
    }

    private void SetGraphicOptions(GraphicsOptions mode) {
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
    }
}