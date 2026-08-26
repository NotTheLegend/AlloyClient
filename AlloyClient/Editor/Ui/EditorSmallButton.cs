using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorSmallButton : Container {
    private readonly CutEdgeRect _background;

    public EditorSmallButton(string text, Action clicked) : base(new ContainerConfig()) {
        var label = new SimpleText(new TextConfig { Text = text, FontSize = 19, FontType = FontType.Bold, Color = 0xFFFFFF });
        var width = label.Width + 10;
        var height = label.Height + 5;
        Resize(width, height);
        _background = new CutEdgeRect(new CutEdgeConfig {
            Width = width, Height = height, CutX = 4, CutY = 4,
            Color = 0x333333, Alpha = 0.8f, MouseEnabled = true,
        });

        AddChild(_background);
        label.X = width / 2;
        label.Y = height / 2;
        label.SetAnchor(UiAnchor.Middle);
        AddChild(label);
        _background.AddEventListener(MouseEvent.MouseOver, () => _background.SetColor(0x565656));
        _background.AddEventListener(MouseEvent.MouseOut, () => _background.SetColor(0x333333));
        _background.AddEventListener(MouseEvent.LeftClick, clicked);
    }
}