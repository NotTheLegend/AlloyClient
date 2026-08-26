using System.Collections.Generic;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Ui.Components.Tooltips;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorPaletteTooltip : Tooltip {
    private const int MinimumTooltipWidth = 180;
    private const int MaximumTooltipWidth = 320;
    private const int Padding = 8;

    public EditorPaletteTooltip(EditorDrawType drawType, EditorCatalogEntry entry)
        : base(MaximumTooltipWidth, 1) {
        var texture = drawType == EditorDrawType.Ground
            ? GroundLibrary.TypeToTextureData.GetValueOrDefault((ushort)entry.Type)
            : ObjectLibrary.TypeToTextureData.GetValueOrDefault((ushort)entry.Type);

        var useEditorTexture = drawType == EditorDrawType.Objects && texture?.EditorTexture.HasValue == true;
        var sheet = useEditorTexture && !string.IsNullOrEmpty(texture.EditorTextureFile)
            ? texture.EditorTextureFile
            : texture?.TextureFile ?? string.Empty;

        var index = useEditorTexture && texture.EditorTextureIndex >= 0
            ? texture.EditorTextureIndex
            : texture?.TextureIndex ?? -1;

        var title = new SimpleText(new TextConfig {
            Text = entry.Name, FontSize = 18, FontType = FontType.Bold,
            Color = 0xFFFFFF, MaxWidth = MaximumTooltipWidth - Padding * 2,
        });

        var typeText = new SimpleText(new TextConfig {
            Text = $"Type: 0x{entry.Type:X}", FontSize = 15, Color = 0xD0D0D0,
        });

        var sheetText = new SimpleText(new TextConfig {
            Text = $"Sheet: {DisplayValue(sheet)}", FontSize = 15,
            Color = 0xD0D0D0, MaxWidth = MaximumTooltipWidth - Padding * 2,
        });

        var indexText = new SimpleText(new TextConfig {
            Text = $"Index: {(index < 0 ? "-" : $"0x{index:X}")}", FontSize = 15, Color = 0xD0D0D0,
        });

        var propsLabel = new SimpleText(new TextConfig {
            Text = "Props:", FontSize = 15, FontType = FontType.Bold, Color = 0xFFFFFF,
        });

        var propsText = new SimpleText(new TextConfig {
            Text = BuildProperties(drawType, entry.Type), FontSize = 14,
            Color = 0xD0D0D0, MaxWidth = MaximumTooltipWidth - Padding * 2,
        });

        SimpleText[] labels = [title, typeText, sheetText, indexText, propsLabel, propsText];

        var y = 7;
        for (var i = 0; i < labels.Length; i++) {
            labels[i].X = Padding;
            labels[i].Y = y;
            y += labels[i].Height + (i == 3 ? 7 : 3);
        }

        var contentWidth = 0;
        foreach (var label in labels) contentWidth = System.Math.Max(contentWidth, label.Width);
        ToolWidth = System.Math.Clamp(contentWidth + Padding * 2, MinimumTooltipWidth, MaximumTooltipWidth);
        ToolHeight = y + 6;

        AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = ToolWidth, Height = ToolHeight, CutX = 6, CutY = 6,
            Color = 0x686868, Alpha = 0.94f,
        }));

        foreach (var label in labels) AddChild(label);
    }

    private static string DisplayValue(string value) => string.IsNullOrEmpty(value) ? "-" : value;

    private static string BuildProperties(EditorDrawType drawType, int type) {
        if (drawType == EditorDrawType.Ground)
            return BuildGroundProperties(GroundLibrary.TypeToGroundProps.GetValueOrDefault((ushort)type));

        return BuildObjectProperties(ObjectLibrary.TypeToObjectProps.GetValueOrDefault((ushort)type));
    }

    private static string BuildObjectProperties(ObjectProperties properties) {
        if (properties is null) return "-";

        var values = new List<string>();
        AddText(values, "Class", properties.Class);
        AddText(values, "Model", properties.Model);
        AddText(values, "Effect", properties.Effect);
        AddFlag(values, "Player", properties.IsPlayer);
        AddFlag(values, "Enemy", properties.IsEnemy);
        AddFlag(values, "Ally", properties.IsAlly);
        AddFlag(values, "Static", properties.Static);
        AddFlag(values, "Occupy", properties.OccupySquare);
        AddFlag(values, "FullOccupy", properties.FullOccupy);
        AddFlag(values, "DrawOnGround", properties.DrawOnGround);
        AddFlag(values, "Container", properties.Container);
        AddFlag(values, "LockedPortal", properties.LockedPortal);
        AddFlag(values, "Skin", properties.Skin);
        AddFlag(values, "NoSkinSelect", properties.NoSkinSelect);
        if (properties.RealSize >= 0) values.Add($"RealSize={properties.RealSize}");
        if (properties.MinSize != 0 || properties.MaxSize != 0) values.Add($"Size={properties.MinSize}-{properties.MaxSize}");
        if (properties.Projectiles.Count > 0) values.Add($"Projectiles={properties.Projectiles.Count}");

        return values.Count == 0 ? "-" : string.Join(", ", values);
    }

    private static string BuildGroundProperties(GroundProperties properties) {
        if (properties is null) return "-";

        var values = new List<string>();
        AddFlag(values, "NoWalk", properties.NoWalk);
        if (properties.MinDamage != 0 || properties.MaxDamage != 0)
            values.Add($"Damage={properties.MinDamage}-{properties.MaxDamage}");

        AddFlag(values, "Push", properties.Push);
        if (properties.Animate.Type != GroundAnimate.State.None)
            values.Add($"Animate={properties.Animate.Type}");

        if (properties.BlendPriority != 0) values.Add($"Blend={properties.BlendPriority}");
        if (properties.CompositePriority != 0) values.Add($"Composite={properties.CompositePriority}");
        if (properties.Speed != 1f) values.Add($"Speed={properties.Speed:0.##}");
        if (properties.SlideAmount != 0f) values.Add($"Slide={properties.SlideAmount:0.##}");
        AddFlag(values, "Sink", properties.Sink);
        AddFlag(values, "Sinking", properties.Sinking);
        AddFlag(values, "RandomOffset", properties.RandomOffset);
        AddFlag(values, "Edge", properties.HasEdge);
        AddFlag(values, "SameTypeEdge", properties.SameTypeEdgeMode);

        return values.Count == 0 ? "-" : string.Join(", ", values);
    }

    private static void AddText(List<string> values, string label, string value) {
        if (!string.IsNullOrWhiteSpace(value)) values.Add($"{label}={value}");
    }

    private static void AddFlag(List<string> values, string label, bool value) {
        if (value) values.Add(label);
    }
}