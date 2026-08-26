using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Display;
using AlloyClient.Utils;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorToolButton : Container {
    public readonly EditorToolType Tool;
    private readonly CutEdgeRect _background;
    private readonly ObjectRect _icon;
    private bool _selected;
    private EditorToolTooltip _tooltip;

    public EditorToolButton(EditorToolType tool, int iconIndex, int y, Action<EditorToolType> clicked)
        : base(new ContainerConfig { Y = y, Width = 30, Height = 26 }) {
        Tool = tool;
        _background = new CutEdgeRect(new CutEdgeConfig {
            Width = 30, Height = 26, CutX = 4, CutY = 4,
            Color = 0x565656, Alpha = 0f, MouseEnabled = true,
        });
        AddChild(_background);
        
        _icon = new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.FromUiAtlas("MapEditor/Tools", iconIndex),
            X = 15, Y = 13, Width = 20, Height = 20, Anchor = UiAnchor.Middle,
            OutlineEnabled = false, GlowEnabled = false,
        });
        AddChild(_icon);
        
        _background.AddEventListener(MouseEvent.MouseOver, OnMouseOver);
        _background.AddEventListener(MouseEvent.MouseOut, OnMouseOut);
        _background.AddEventListener(MouseEvent.LeftClick, () => clicked(Tool));
    }

    public void SetSelected(bool selected) {
        _selected = selected;
        _background.Alpha = selected ? 0.8f : 0f;
        _icon.Alpha = selected ? 1f : 0.55f;
    }

    private void OnMouseOver() {
        _background.Alpha = _selected ? 0.9f : 0.55f;
        GetTooltipText(out var title, out var keybind, out var hint);
        _tooltip = new EditorToolTooltip(title, keybind, hint);
        TooltipManager.AddTooltip(_tooltip);
    }

    private void OnMouseOut() {
        _background.Alpha = _selected ? 0.8f : 0f;
        if (_tooltip is null) return;

        TooltipManager.RemoveTooltip(_tooltip);
        _tooltip = null;
    }

    private void GetTooltipText(out string title, out string keybind, out string hint) {
        switch (Tool) {
            case EditorToolType.Select:
                title = "Select";
                keybind = "S";
                hint = "Drag to select; drag inside to move.";
                break;
            case EditorToolType.Pencil:
                title = "Pencil";
                keybind = "D";
                hint = "Ctrl + Scroll to change brush size.";
                break;
            case EditorToolType.Line:
                title = "Line";
                keybind = "L";
                hint = "Drag to draw a straight line.";
                break;
            case EditorToolType.Shape:
                title = "Shape";
                keybind = "U";
                hint = "Drag to draw a filled shape.";
                break;
            case EditorToolType.Bucket:
                title = "Bucket";
                keybind = "F";
                hint = "Fill connected matching tiles.";
                break;
            case EditorToolType.Picker:
                title = "Picker";
                keybind = "A";
                hint = "Pick the tile under the cursor.";
                break;
            case EditorToolType.Eraser:
                title = "Eraser";
                keybind = "E";
                hint = "Ctrl + Scroll to change brush size.";
                break;
            default:
                title = "Edit";
                keybind = "I";
                hint = "Edit the object on a tile.";
                break;
        }
    }
}