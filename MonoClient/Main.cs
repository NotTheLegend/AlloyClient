using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Common.Pipeline;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.Assets;
using MonoClient.Assets.Libraries;
using MonoClient.Display;
using MonoClient.Rendering;
using MonoClient.Screens;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.State;
using MonoClient.Ui;
using MonoClient.UiLib;
using MonoClient.UiLib.Input;
using MonoClient.UiLib.Utils.Signals;
using MonoClient.Utils;
using Easing = MonoClient.UiLib.Easing;
using KeyboardInput = MonoClient.UiLib.Input.KeyboardInput;
using MouseInput = MonoClient.UiLib.Input.MouseInput;

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
        Window.Title = "Domain of Magica";

        Graphics.IsFullScreen = Settings.Fullscreen;
        Graphics.HardwareModeSwitch = !Settings.Fullscreen;
        Graphics.PreferredBackBufferWidth = Settings.ScreenWidth;
        Graphics.PreferredBackBufferHeight = Settings.ScreenHeight;
        Graphics.ApplyChanges();
        
        

        Map.GraphicsDevice = Graphics.GraphicsDevice;

        base.Initialize();
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.Byte[]")]
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: Microsoft.Xna.Framework.Color[]")]
    protected override void LoadContent() {
        Atlas = ContentManager.Load<MainAtlas>("atlas");
        UiAtlas = ContentManager.Load<UiAtlas>("AtlasUi");
        MinimapData.Init(GraphicsDevice, out var mapTexture);
        
        ModelData.Load();
        
        //UiRender needs to be loaded first so Render can pull font data from it
        UiRender.ConfigureAndLoad(this, ContentManager, GraphicsDevice, Atlas, UiAtlas, mapTexture, new IntVector2(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight));
        UiRender.UpdateViewMatrix(Settings.ScreenWidth, Settings.ScreenHeight);
        Render.FirstTimeInit();
        
        SliceConfig.LoadSliceData();
        
        DisplayManager.Init();
    }

    protected override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (_lastScreenWidth != Settings.ScreenWidth || _lastScreenHeight != Settings.ScreenHeight) {
            _lastScreenWidth = Settings.ScreenWidth;
            _lastScreenHeight = Settings.ScreenHeight;
            ScreenResized?.Invoke();
        }
        
        // Only update inputs if window is active
        if (IsActive) {
            MouseInput.Update();
            KeyboardInput.Update();
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