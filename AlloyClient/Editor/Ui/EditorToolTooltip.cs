using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Ui.Components.Tooltips;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorToolTooltip : Tooltip {
    public EditorToolTooltip(string title, string keybind, string hint)
        : base(250, string.IsNullOrEmpty(hint) ? 51 : 73) {
        AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = ToolWidth, Height = ToolHeight, CutX = 6, CutY = 6,
            Color = 0x686868, Alpha = 0.94f
        }));
        AddChild(new SimpleText(new TextConfig {
            Text = title, FontSize = 18, FontType = FontType.Bold,
            X = 8, Y = 6, Color = 0xFFFFFF
        }));
        AddChild(new SimpleText(new TextConfig {
            Text = $"Keybind: {keybind}", FontSize = 15, FontType = FontType.Normal,
            X = 8, Y = 28, Color = 0xD0D0D0
        }));
        if (!string.IsNullOrEmpty(hint)) {
            AddChild(new SimpleText(new TextConfig {
                Text = hint, FontSize = 15, FontType = FontType.Normal,
                X = 8, Y = 48, Color = 0xD0D0D0
            }));
        }
    }
}
