using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;

namespace MonoClient.Ui.Components.Tooltips;

public abstract class Tooltip : Sprite {

    protected Tooltip() {
        TooltipMode = true;
        var _ = new NineSliceRect(new NineSliceConfig {
            SliceData = SliceConfig.TooltipBackground,
            Padding = false,
            CutX = 10,
            CutY = 10,
            X = 100,
            Y = 100,
            Width = 200,
            Height = 400,
            MouseEnabled = true
        });
    }
    
}