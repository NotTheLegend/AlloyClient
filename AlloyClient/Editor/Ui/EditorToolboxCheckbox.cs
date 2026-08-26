using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorToolboxCheckbox : Container {
    private readonly SimpleText _label;
    private readonly CutEdgeRect _box;
    private readonly CutEdgeRect _fill;

    public EditorToolboxCheckbox(string text, int y, bool initialState, Action clicked)
        : base(new ContainerConfig { Y = y, Width = 106, Height = 28 }) {
        _label = new SimpleText(new TextConfig {
            Text = text, FontSize = 17, FontType = FontType.Bold,
            X = 7, Y = 14, Anchor = UiAnchor.MiddleLeft, Color = 0xFFFFFF,
        });
        AddChild(_label);

        _box = new CutEdgeRect(new CutEdgeConfig {
            X = 84, Y = 3, Width = 22, Height = 22,
            CutX = 4, CutY = 4, Color = 0xFFFFFF,
        });
        AddChild(_box);
        
        AddChild(new CutEdgeRect(new CutEdgeConfig {
            X = 86, Y = 5, Width = 18, Height = 18,
            CutX = 3, CutY = 3, Color = 0x565656,
        }));

        _fill = new CutEdgeRect(new CutEdgeConfig {
            X = 90, Y = 9, Width = 10, Height = 10,
            CutX = 2, CutY = 2, Color = 0xFFFFFF,
        });
        _fill.Visible = initialState;
        AddChild(_fill);

        var hit = new ColorRect(new ColorRectConfig {
            Width = 106, Height = 28, Color = 0xFFFFFF, Alpha = 0f, MouseEnabled = true,
        });

        AddChild(hit);
        hit.AddEventListener(MouseEvent.MouseOver, OnMouseOver);
        hit.AddEventListener(MouseEvent.MouseOut, OnMouseOut);
        hit.AddEventListener(MouseEvent.LeftClick, clicked);
    }

    public void SetChecked(bool value) {
        _fill.Visible = value;
    }

    private void OnMouseOver() {
        _label.SetColor(0xFFDC85);
        _box.SetColor(0xFFDC85);
    }

    private void OnMouseOut() {
        _label.SetColor(0xFFFFFF);
        _box.SetColor(0xFFFFFF);
    }
}