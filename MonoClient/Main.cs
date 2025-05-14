using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Common.Pipeline;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoClient.AppEngine;
using MonoClient.Assets;
using MonoClient.Assets.Libraries;
using MonoClient.Display;
using MonoClient.Rendering;
using MonoClient.Screens;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.State;
using MonoClient.Ui;
using MonoClient.UiLib;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Input;
using MonoClient.UiLib.Signals;
using MonoClient.Utils;
using Easing = MonoClient.UiLib.Extra.Easing;
using KeyboardInput = MonoClient.UiLib.Input.KeyboardInput;

namespace MonoClient;

public class Main : Game {
    private static readonly Logger Log = new(typeof(Main));

    public static ContentManager ContentManager;

    public static GraphicsDeviceManager Graphics;

    public static readonly SingleSignal<GraphicsOptions> GraphicsMode = new();

    public static Main GameInstance;
    public static MainAtlas Atlas;
    public static UiAtlas UiAtlas;

    public static Action ScreenResized;
    private static int _lastScreenWidth;
    private static int _lastScreenHeight;

    public Main() {
        Graphics = new GraphicsDeviceManager(this);

        ContentManager = Content;
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

        base.Initialize();
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: Microsoft.Xna.Framework.Color[]")]
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.Byte[]; size: 65MB")]
    protected override void LoadContent() {
        Atlas = ContentManager.Load<MainAtlas>("atlas");
        UiAtlas = ContentManager.Load<UiAtlas>("AtlasUi");
        MinimapTexture.Init(GraphicsDevice, out var mapTexture);
        
        ModelData.Load();

        var settings = new UiSettings {
            Game = this,
            GameAtlas = Atlas,
            UiAtlas = UiAtlas,
            MinimumScreen = new IntVector2(800, 600),
            DefaultScreen = new IntVector2(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight)
        };

        //UiRender needs to be loaded first so Render can pull font data from it
        UiRender.ConfigureAndLoad(settings, out var stage);
        
        UiRender.RegisterTexture(TextureType.GameAtlas, Atlas.Texture);
        UiRender.RegisterTexture(TextureType.UiAtlas, UiAtlas.Texture);
        UiRender.RegisterTexture(TextureType.Minimap, mapTexture);
        UiRender.RegisterTexture(TextureType.TitleBackground, Content.Load<Texture2D>("Ui/titleView/TitleScreenBackground"));
        UiRender.RegisterTexture(TextureType.TitleGraphic, Content.Load<Texture2D>("Ui/titleView/TitleScreenGraphic"));
        
        Render.FirstTimeInit();
        
        SliceConfig.LoadSliceData();
        
        DisplayManager.Init(stage);

        ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
        //ScreenManager.FadeToScreen(new TestScreen(), Easing.SineInOut, 1000, 0x0);


        /* uncomment to force restart app/world servers, takes around 1 minute for reboot to finish */
        //AppEngineClient.SendRequest("/dev/backup/restart");
    }

    protected override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (_lastScreenWidth != Settings.ScreenWidth || _lastScreenHeight != Settings.ScreenHeight) {
            _lastScreenWidth = Settings.ScreenWidth;
            _lastScreenHeight = Settings.ScreenHeight;
            ScreenResized?.Invoke();
        }
        
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