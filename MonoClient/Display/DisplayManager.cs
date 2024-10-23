using Microsoft.Xna.Framework;
using MonoClient.UiLib;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.Display;

public static class DisplayManager {
    
    public static void Update(GameTime gameTime) {
        // Tick tweens before ui update cycle
        GTween.Update(gameTime);
        
        // Update Layers (Lowest to Highest)
        // DisplayState is used to track which layers need  mouse events
        var state = DisplayState.None;
        ScreenManager.Update(gameTime, ref state);
        PanelManager.Update(gameTime, ref state);
        DialogManager.Update(gameTime, ref state);
        TooltipManager.Update(gameTime);
        
        // Handle Mouse Events (Highest to Lowest)
        var consumed = (MouseEventId) 0;
        switch (state) {
            case DisplayState.Dialog:
                DialogManager.HandleMouseEvents(ref consumed);
                break;
            case DisplayState.Panel:
                PanelManager.HandleMouseEvents(ref consumed);
                break;
            case DisplayState.Screen:
                ScreenManager.HandleMouseEvents(ref consumed);
                break;
        }
    }

    public static void Draw(GameTime gameTime) {
        UiRender.LastRenderCount = 0;
        // Draw layers (Lowest to Highest)
        ScreenManager.Draw(gameTime);
        PanelManager.Draw(gameTime);
        DialogManager.Draw(gameTime);
        TooltipManager.Draw(gameTime);
    }
    
}

public enum DisplayState {
    None,
    Screen,
    Panel,
    Dialog
}