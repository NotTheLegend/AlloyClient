using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Ui.Components.Tooltips;

public abstract class Tooltip : UiElement {

    private NineSliceRect TooltipSprite;
    private NineSliceConfig TooltipConfig;


    private Container Contain;

    public int ToolWidth;
    public int ToolHeight;
    
    protected Tooltip(int width, int height) {
        ClampToStage = true;
        MouseEnabled = false;
        MouseChildren = false;

        ToolWidth = width;
        ToolHeight = height;

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
            Height = ToolHeight * 1000
        };
        TooltipSprite = new NineSliceRect(TooltipConfig);
        Contain.AddChild(TooltipSprite);
    }

    protected override void OnAddedToStage() {
        AddEventListener(Event.EnterFrame, OnEnterFrame);
    }

    protected override void OnRemovedFromStage() {
        RemoveEventListener(Event.EnterFrame, OnEnterFrame);
    }

    private void OnEnterFrame() {
        (X, Y) = Stage.Mouse.GetMousePosition();
        Anchor = AnchorFromQuadrants(X < Stage.StageWidth / 2, Y < Stage.StageHeight / 2);
    }

    private static UiAnchor AnchorFromQuadrants(bool x, bool y) => (x, y) switch {
        (true, true) => UiAnchor.TopLeft,
        (false, true) => UiAnchor.TopRight,
        (true, false) => UiAnchor.BottomLeft,
        (false, false) => UiAnchor.BottomRight,
    };
}