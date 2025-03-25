using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using System.Reflection.Metadata.Ecma335;

namespace MonoClient.Ui.Components.Tooltips;

public abstract class Tooltip : Sprite {

    private NineSliceRect TooltipSprite;
    private NineSliceConfig TooltipConfig;


    private Container Contain;

    public int ToolWidth;
    public int ToolHeight;
    protected Tooltip(int width, int height) {
        TooltipMode = true;

        ToolWidth = width;
        ToolHeight = height;

        Width = width;
        Height = height;

        Contain = new Container();
        AddChild(Contain);
    }

    public virtual void DrawSprite()
    {
        TooltipConfig = new NineSliceConfig
        {
            SliceData = SliceConfig.TooltipBackgroundSmall,
            Padding = false,
            CutX = 5,
            CutY = 5,
            Width = ToolWidth,
            Height = ToolHeight
        };
        TooltipSprite = new NineSliceRect(TooltipConfig);
        Contain.AddChild(TooltipSprite);
        Height = ToolHeight;
        SetBaseDimensions(ToolWidth, ToolHeight);
    }
    
}