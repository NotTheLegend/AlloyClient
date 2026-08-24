using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.Assets.Libraries;
using AlloyClient.Ui;
using AlloyClient.Ui.Components.Graphics;
using AlloyClient.Utils;

namespace AlloyClient.Screens.Components.CharacterSelection;

public sealed class ClassChoiceCard : Container {
    public const int CardWidth = 200;
    public const int CardHeight = 150;

    private const uint BackgroundColor = 0x2B2B2B;
    private const uint HoverColor = 0x4A4A4A;

    private readonly ColorRect _background;

    public ushort Type { get; }

    public ClassChoiceCard(ushort type, int bestFame, Action<ClassChoiceCard> onSelected)
        : base(new ContainerConfig { Width = CardWidth, Height = CardHeight }) {
        Type = type;

        _background = new ColorRect(new ColorRectConfig {
            Width = CardWidth,
            Height = CardHeight,
            Color = BackgroundColor,
            Alpha = 0.82f
        });

        AddChild(_background);
        AddChild(new CutCornerOutline(CardWidth, CardHeight));

        var textureData = ObjectLibrary.TypeToTextureData[type];
        var frames = textureData.AnimatedTextures.FaceRight;
        var texture = frames is { Length: > 0 } ? frames[0] : textureData.Texture;
        AddChild(new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.Create(texture, TextureType.GameAtlas),
            X = CardWidth / 2,
            Y = 42,
            Width = 58,
            Height = 58,
            Anchor = UiAnchor.Middle,
            OutlineEnabled = false,
            GlowEnabled = false
        }));

        AddStars(FameUtils.FameToStar(bestFame));

        var props = ObjectLibrary.TypeToObjectProps[type];
        AddChild(new SimpleText(new TextConfig {
            Text = props.DisplayName,
            FontSize = 20,
            FontType = FontType.Bold,
            Color = 0xFFFFFF,
            X = CardWidth / 2,
            Y = 124,
            MaxWidth = CardWidth - 16,
            Anchor = UiAnchor.Middle
        }));

        MouseEnabled = true;
        AddEventListener(MouseEvent.MouseOver, () => _background.SetColor(HoverColor));
        AddEventListener(MouseEvent.MouseOut, () => _background.SetColor(BackgroundColor));
        AddEventListener(MouseEvent.LeftClick, () => onSelected(this));
    }

    private void AddStars(int earnedStars) {
        const int size = 16;
        const int startX = CardWidth / 2 - size * 2;
        for (var i = 0; i < FameUtils.StarFameRequirements.Length; i++) {
            var star = new ObjectRect(new ObjectRectConfig {
                Texture = TextureHelper.FromUiAtlas("CharacterList/StarGraphic"),
                X = startX + i * size,
                Y = 91,
                Width = size,
                Height = size,
                Anchor = UiAnchor.Middle,
                OutlineEnabled = false,
                GlowEnabled = false
            });

            if (i >= earnedStars) {
                star.ColorTransformation = new ColorTransform(0.45f, 0.45f, 0.45f, 1f);
            }

            AddChild(star);
        }
    }
}