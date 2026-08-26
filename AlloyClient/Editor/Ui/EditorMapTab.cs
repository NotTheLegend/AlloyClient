using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorMapTab : Container {
    public readonly int MapId;

    private readonly CutEdgeRect _background;
    private readonly SimpleText _label;
    private readonly SimpleText _close;
    private bool _selected;

    public EditorMapTab(int mapId, string text, Action selected, Action closed)
        : base(new ContainerConfig { Width = 150, Height = 28 }) {
        MapId = mapId;
        MouseEnabled = true;

        _background = new CutEdgeRect(new CutEdgeConfig {
            Width = 150,
            Height = 28,
            CutX = 5,
            CutY = 5,
            Color = 0x333333,
            Alpha = 0.9f,
            MouseEnabled = true,
        });

        AddChild(_background);

        _label = new SimpleText(new TextConfig {
            Text = text,
            FontSize = 16,
            FontType = FontType.Bold,
            X = 5,
            Y = 14,
            Anchor = UiAnchor.MiddleLeft,
            Color = 0xFFFFFF,
            MaxWidth = 118,
        });

        AddChild(_label);

        _close = new SimpleText(new TextConfig {
            Text = "x",
            FontSize = 16,
            FontType = FontType.Bold,
            X = 140,
            Y = 14,
            Anchor = UiAnchor.Middle,
            Color = 0xFFFFFF,
        });

        AddChild(_close);

        var closeHit = new ColorRect(new ColorRectConfig {
            X = 127,
            Width = 23,
            Height = 28,
            Color = 0xFFFFFF,
            Alpha = 0f,
            MouseEnabled = true,
        });

        closeHit.AddEventListener(MouseEvent.LeftClick, args => {
            args.StopImmediatePropagation();
            closed();
        });

        AddChild(closeHit);

        AddEventListener(MouseEvent.MouseOver, () => _background.Alpha = _selected ? 1f : 0.7f);
        AddEventListener(MouseEvent.MouseOut, () => _background.Alpha = _selected ? 0.9f : 0.35f);
        AddEventListener(MouseEvent.LeftClick, selected);
    }

    public void SetSelected(bool selected) {
        _selected = selected;
        _background.SetColor(selected ? 0x565656u : 0x333333u);
        _background.Alpha = selected ? 0.9f : 0.35f;
    }

    public void SetText(string text) => _label.SetText(text);
}