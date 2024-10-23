using Microsoft.Xna.Framework;
using MonoClient.Ui.Components.Tooltips;
using MonoClient.UiLib.BuiltIn;

namespace MonoClient.Display;

public static class TooltipManager {
    
    private static readonly DisplayContainer Container = new DisplayContainer();
    
    private static Tooltip _current = null;

    public static void Update(GameTime gameTime) {
        Container.Update(gameTime);
    }

    public static void Draw(GameTime gameTime) {
        Container.Draw(gameTime);
    }

    public static void AddTooltip(Tooltip tooltip) {
        if (_current != null)
            Container.RemoveChild(_current);

        _current = tooltip;
        Container.AddChild(_current);
    }
    
    public static void RemoveTooltip(Tooltip tooltip) {
        if (_current != tooltip) return;
        Container.RemoveChild(_current);
    }
}