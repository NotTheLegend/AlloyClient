using System;
using AlloyClient.Game;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using Alloy.Engine;
using AlloyClient.Ui.Components;
using OpenTK.Platform;

namespace AlloyClient.Display;

public sealed class ScreenManager : Sprite {
    private static ScreenManager _instance;
    public readonly static FadeScreen FadeScreen = new(0);

    private static Screen _prevScreen;
    private static Screen _currScreen = FadeScreen;

    public ScreenManager() {
        _instance = this;
        FadeScreen.Visible = false;
        _instance.AddChild(FadeScreen);
        
        AddEventListener(Event.AddedToStage, AddedToStage);
    }

    private void AddedToStage() {
        Stage.AddEventListener(KeyboardEvent.KeyUp, OnKeyUp);
    }

    /// <summary>
    /// Calls the current screens virtual update call, used for drawing the actual game
    /// </summary>
    public static void Update(GameTime gameTime) => _currScreen?.Update(gameTime);
    /// <summary>
    /// Calls the current screens virtual draw call, used for drawing the actual game
    /// </summary>
    public static void Draw(GameTime gameTime) => _currScreen?.Draw(gameTime);

    public static void SetScreen(Screen screen) {
        RemovePrevious();
        _currScreen = screen;
        _instance.AddChild(_currScreen);
    }

    public static void SetPrevious() {
        SetScreen(_prevScreen);
    }

    public static void FadeToScreen(Screen screen, Easing ease, int durationMs, uint color, Action onFinish = null) {
        Main.OnScreenChange.Dispatch(screen is GameScreen ? ScreenType.Game : ScreenType.Menu);

        FadeScreen.Visible = true;
        FadeScreen.SetFadeColor(color);
        screen.Alpha = 0f;
        GTween.Add(Tween.New(_currScreen, ease, durationMs / 2, 0f, EaseType.Alpha, 0, () => { onFinish?.Invoke(); SetScreen(screen); }));
        GTween.Add(Tween.New(screen, ease, durationMs / 2, 1f, EaseType.Alpha, durationMs / 2, () => { FadeScreen.Visible = false; }));
    }

    public static void FadeTo(Screen screen, Action callback = null) => FadeToScreen(screen, Easing.SineInOut, 1000, 0x0, callback);

    public static void FadeToPrevious(Easing ease, int durationMs, uint color) {
        FadeToScreen(_prevScreen, ease, durationMs, color);
    }

    private static void RemovePrevious() {
        _prevScreen = _currScreen;
        _instance.RemoveChild(_currScreen);
    }

    private void OnKeyUp(KeyboardEvent args) {
        if ((args.Key == Key.Return && args.Alt) || Settings.FullscreenKey.Equals(args.Code)) {
            Settings.FullscreenState.Set(!Settings.FullscreenState);
            Main.OnFullscreenToggle.Dispatch();
        }
    }
}

public abstract class Screen : UiElement {
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