using Microsoft.Xna.Framework;
using MonoClient.Screens;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Utils;

namespace MonoClient.Display;

public static class DisplayManager {

    private static readonly DisplayContainer Screen = new();

    static DisplayManager() {
        Screen.AddChild(ScreenManager.FadeScreen);
        Screen.AddChild(new ScreenManager());
        Screen.AddChild(new OverlayManager());
        Screen.AddChild(new DialogManager());
        Screen.AddChild(new TooltipManager());
    }

    public static void Init() {
        //ScreenManager.FadeToScreen(new TestScreen(), Easing.SineInOut, 500, 0x0);
        ScreenManager.FadeToScreen(new LoadingScreen(), Easing.SineInOut, 1000, 0x0);
    }
    
    public static void Update(GameTime gameTime) {
        GTween.Update(gameTime);
        Timer.Update(gameTime);
        ScreenManager.Update(gameTime);
        Screen.Update(gameTime);
    }

    public static void Draw(GameTime gameTime) {
        UiRender.LastRenderCount = 0;
        ScreenManager.Draw(gameTime);
        Screen.Draw(gameTime);
    }
    
}