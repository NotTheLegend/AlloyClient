using System.Reflection.Metadata.Ecma335;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Ui.Components.Tooltips;

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
            SliceData = SliceLibrary.TooltipBackgroundSmall,
            CutX = 5,
            CutY = 5,
            Width = ToolWidth,
            Height = ToolHeight
        };
        TooltipSprite = new NineSliceRect(TooltipConfig);
        Contain.AddChild(TooltipSprite);
        Height = ToolHeight;
        //todo:SetBaseDimensions(ToolWidth, ToolHeight);
    }
    
}