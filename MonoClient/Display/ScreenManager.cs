using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoClient.Screens.Game;
using MonoClient.Screens.MapEditor;
using MonoClient.State;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Input;

namespace MonoClient.Display;

public sealed class ScreenManager : Sprite {
    private static ScreenManager _instance;
    public static readonly FadeScreen FadeScreen = new(0);

    private static Screen _prevScreen;
    private static Screen _currScreen = FadeScreen;

    public ScreenManager() {
        _instance = this;
        FadeScreen.Visible = false;
        SetScreen(new FadeScreen(0));
    }

    /// <summary>
    /// Calls the current screens virtual update call, used for drawing the actual game
    /// </summary>
    public static void Update(GameTime gameTime) => _currScreen.Update(gameTime);
    /// <summary>
    /// Calls the current screens virtual draw call, used for drawing the actual game
    /// </summary>
    public static void Draw(GameTime gameTime) => _currScreen.Draw(gameTime);

    public static void SetScreen(Screen screen) {
        RemovePrevious();
        _currScreen = screen;
        _instance.AddChild(_currScreen);
    }

    public static void SetPrevious() {
        SetScreen(_prevScreen);
    }

    public static void FadeToScreen(Screen screen, Easing ease, int durationMs, uint color, Action onFinish = null) {
        Main.GraphicsMode.Dispatch(screen is GameScreen or MapEditorScreen ? GraphicsOptions.InGame : GraphicsOptions.TitleScreen);

        FadeScreen.Visible = true;
        FadeScreen.SetFadeColor(color);
        screen.Alpha = 0f;
        GTween.Add(Tween.New(_currScreen, ease, durationMs / 2, 0f, EaseType.Alpha, 0, () => { onFinish?.Invoke(); SetScreen(screen); }));
        GTween.Add(Tween.New(screen, ease, durationMs / 2, 1f, EaseType.Alpha, durationMs / 2, () => { FadeScreen.Visible = false; }));
    }

    public static void FadeToPrevious(Easing ease, int durationMs, uint color) {
        FadeToScreen(_prevScreen, ease, durationMs, color);
    }

    private static void RemovePrevious() {
        _prevScreen = _currScreen;
        _instance.RemoveChild(_currScreen);
    }

    protected override void OnUpdate(GameTime gameTime) {
        HandleGlobalCommands();
    }

    private static void HandleGlobalCommands() {
        // Fullscreen toggle
        if (KeyboardInput.AreKeysPressed(Keys.LeftAlt, Keys.Enter) || KeyboardInput.IsKeyPressed(Keys.F11)) {
            Main.Graphics.IsFullScreen = !Main.Graphics.IsFullScreen;
            Main.Graphics.HardwareModeSwitch = !Main.Graphics.IsFullScreen;

            int width, height;
            if (Main.Graphics.IsFullScreen) {
                width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else {
                width = Settings.NonFullscreenWidth;
                height = Settings.NonFullscreenHeight;
            }

            Main.Graphics.PreferredBackBufferWidth = width;
            Main.Graphics.PreferredBackBufferHeight = height;
            Main.Graphics.ApplyChanges();

            Settings.Fullscreen = Main.Graphics.IsFullScreen;
            Settings.SetWindowSize(width, height);
            Settings.SaveSettings();
            UiRender.UpdateViewMatrix(width, height);
        }
    }
}

public abstract class Screen : Sprite {
    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(GameTime gameTime) { }
}

public class FadeScreen : Screen {
    
    private readonly ColorRect _rect;

    public FadeScreen(uint color) {
        var config = new ColorRectConfig { X = 0, Y = 0, Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = color};
        _rect = new ColorRect(config);
        AddChild(_rect);
    }

    public void SetFadeColor(uint color) => _rect.SetColor(color);
}