using Microsoft.Xna.Framework;
using MonoClient.Screens;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Utils;

namespace MonoClient.Display;

public static class DisplayManager {

    private static Stage _stage;

    public static void Init(Stage stage) {
        _stage = stage;
        _stage.AddChild(ScreenManager.FadeScreen);
        _stage.AddChild(new ScreenManager());
        _stage.AddChild(new OverlayManager());
        _stage.AddChild(new DialogManager());
        _stage.AddChild(new TooltipManager());
    }

    public static void Start() {
        //ScreenManager.FadeToScreen(new TestScreen(), Easing.SineInOut, 500, 0x0);
        ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
    }
    
    public static void Update(GameTime gameTime) {
        ScreenManager.Update(gameTime);
        _stage.Update(gameTime);
    }

    public static void Draw(GameTime gameTime) {
        UiRender.LastRenderCount = 0;
        ScreenManager.Draw(gameTime);
        _stage.Draw(gameTime);
    }
    
}