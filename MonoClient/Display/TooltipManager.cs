using Microsoft.Xna.Framework;
using MonoClient.Ui.Components.Tooltips;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;

namespace MonoClient.Display;

public sealed class TooltipManager : Sprite {

    private static TooltipManager _instance;
    
    private static Tooltip _current;

    public TooltipManager() {
        _instance = this;
    }

    public static void AddTooltip(Tooltip tooltip) {
        if (_current != null)
            _instance.RemoveChild(_current);

        _current = tooltip;
        _instance.AddChild(_current);
    }
    
    public static void RemoveTooltip(Tooltip tooltip) {
        if (_current != tooltip) return;
        _instance.RemoveChild(_current);
    }
}