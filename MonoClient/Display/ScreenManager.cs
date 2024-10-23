using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoClient.Screens.Game;
using MonoClient.State;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Input;

namespace MonoClient.Display;

public static class ScreenManager {
    private static readonly FadeScreen FadeScreen = new();

    private static Screen _prevScreen;
    private static Screen _currScreen = FadeScreen;

    private static bool _fadeActive;

    public static void SetScreen(Screen screen) {
        _prevScreen = _currScreen;
        _currScreen = screen;
    }

    public static void FadeToScreen(Screen screen, Easing ease, int durationMs, uint color, Action onFinish = null) {
        if (screen is GameScreen) {
            Main.GameInstance.SetInGameGraphics();
        }
        else {
            Main.GameInstance.SetTitleGraphics();
        }
        
        _fadeActive = true;
        FadeScreen.SetFadeColor(color);
        screen.Alpha = 0f;
        GTween.Add(Tween.New(_currScreen, ease, durationMs / 2, 0f, EaseType.Alpha, 0, () => { onFinish?.Invoke(); SetScreen(screen); }));
        GTween.Add(Tween.New(screen, ease, durationMs / 2, 1f, EaseType.Alpha, durationMs / 2, () => { _fadeActive = false; }));
    }

    public static void FadeToPrevious(Easing ease, int durationMs, uint color) {
        FadeToScreen(_prevScreen, ease, durationMs, color);
    }

    public static void Update(GameTime gameTime, ref DisplayState state) {
        HandleGlobalCommands();
        
        if (_fadeActive) {
            FadeScreen.Update(gameTime);
        }

        _currScreen.Update(gameTime);
        state = DisplayState.Screen;
    }

    public static void HandleMouseEvents(ref MouseEventId consumed) {
        _currScreen.HandleMouseEvents(ref consumed);
    }

    public static void Draw(GameTime gameTime) {
        if (_fadeActive) {
            FadeScreen.Draw(gameTime);
        }
        
        _currScreen.Draw(gameTime);
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

public class FadeScreen : Screen {
    
    private readonly ColorRect _rect;

    public FadeScreen() {
        var config = new ColorRectConfig { X = 0, Y = 0, Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x0};
        _rect = new ColorRect(config);
        AddChild(_rect);
    }

    public void SetFadeColor(uint color) => _rect.SetColor(color);
}