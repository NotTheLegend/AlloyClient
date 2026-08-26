using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorSmallButton : Container {

    public EditorSmallButton(string text, Action clicked) : base(new ContainerConfig()) {
        var label = new SimpleText(new TextConfig { Text = text, FontSize = 19, FontType = FontType.Bold, Color = 0xFFFFFF });
        var width = label.Width + 10;
        var height = label.Height + 5;
        Resize(width, height);
        
        var background = new CutEdgeRect(new CutEdgeConfig {
            Width = width, Height = height, CutX = 4, CutY = 4,
            Color = 0x333333, Alpha = 0.8f, MouseEnabled = true,
        });
        AddChild(background);
        
        label.X = width / 2;
        label.Y = height / 2;
        label.SetAnchor(UiAnchor.Middle);
        AddChild(label);
        
        background.AddEventListener(MouseEvent.MouseOver, () => background.SetColor(0x565656));
        background.AddEventListener(MouseEvent.MouseOut, () => background.SetColor(0x333333));
        background.AddEventListener(MouseEvent.LeftClick, clicked);
    }
}