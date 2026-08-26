using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Ui.Components.Graphics;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorToolboxAction : Container {
    public readonly EditorDrawType DrawType;
    private readonly SimpleText _label;
    private readonly CutEdgeRect _selection;
    private readonly CutCornerOutline _outline;
    private bool _selected;

    public EditorToolboxAction(string text, EditorDrawType drawType, int y, Action<EditorDrawType> clicked)
        : base(new ContainerConfig { Y = y, Width = 106, Height = 26 }) {
        DrawType = drawType;
        _selection = new CutEdgeRect(new CutEdgeConfig {
            Width = 106, Height = 26, CutX = 5, CutY = 5,
            Color = 0x777777, Alpha = 0.58f
        });
        _selection.Visible = false;
        AddChild(_selection);
        _outline = new CutCornerOutline(106, 26, 0xFFFFFF, 0.55f);
        _outline.Visible = false;
        AddChild(_outline);
        _label = new SimpleText(new TextConfig {
            Text = text, FontSize = 17, FontType = FontType.Bold,
            X = 53, Y = 13, Anchor = UiAnchor.Middle, Color = 0xB3B3B3
        });
        AddChild(_label);

        var hit = new ColorRect(new ColorRectConfig {
            Width = 106, Height = 26, Color = 0xFFFFFF, Alpha = 0f, MouseEnabled = true
        });
        AddChild(hit);
        hit.AddEventListener(MouseEvent.MouseOver, () => _label.SetColor(0xFFDC85));
        hit.AddEventListener(MouseEvent.MouseOut, () => _label.SetColor(_selected ? 0xFFFFFFu : 0xB3B3B3u));
        hit.AddEventListener(MouseEvent.LeftClick, () => clicked(DrawType));
    }

    public void SetSelected(bool selected) {
        _selected = selected;
        _selection.Visible = selected;
        _outline.Visible = selected;
        _label.SetColor(selected ? 0xFFFFFFu : 0xB3B3B3u);
    }
}
