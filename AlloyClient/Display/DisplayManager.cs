using Common;
using RealmClient.UiLib;
using RealmClient.UiLib.Core;

namespace RealmClient.Display;

public static class DisplayManager {

    private static Stage _stage;

    public static void Init(Stage stage) {
        if (_stage != null)
            return;
        
        _stage = stage;
        _stage.AddChild(ScreenManager.FadeScreen);
        _stage.AddChild(new ScreenManager());
        _stage.AddChild(new OverlayManager());
        _stage.AddChild(new DialogManager());
        _stage.AddChild(new TooltipManager());
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