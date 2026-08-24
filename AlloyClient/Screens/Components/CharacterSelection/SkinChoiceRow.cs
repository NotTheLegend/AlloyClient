using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Assets.Libraries;
using AlloyClient.Utils;

namespace AlloyClient.Screens.Components.CharacterSelection;

public sealed class SkinChoiceRow : Container {
    public const int RowHeight = 65;

    private const uint BackgroundColor = 0x242424;
    private const uint HoverColor = 0x454545;
    private const uint SelectedColor = 0x858585;

    private readonly ColorRect _background;
    private readonly Container _selectionMarker;
    private readonly CutEdgeRect _selectionInterior;
    private readonly CutEdgeRect _selectionFill;
    private readonly bool _locked;
    private bool _selected;

    public ushort SkinType { get; }

    public SkinChoiceRow(int width, ushort textureType, ushort skinType, string name, bool locked,
        Action<SkinChoiceRow> onSelected)
        : base(new ContainerConfig { Width = width, Height = RowHeight }) {
        SkinType = skinType;
        _locked = locked;

        _background = new ColorRect(new ColorRectConfig {
            Width = width,
            Height = RowHeight,
            Color = BackgroundColor,
            Alpha = 0.88f
        });

        AddChild(_background);

        if (ObjectLibrary.TypeToTextureData.TryGetValue(textureType, out var textureData) && textureData != null) {
            var faceRight = textureData.AnimatedTextures.FaceRight;
            var faceDown = textureData.AnimatedTextures.FaceDown;
            var texture = faceRight is { Length: > 0 }
                ? faceRight[0]
                : faceDown is { Length: > 0 }
                    ? faceDown[0]
                    : textureData.Texture;

            if (texture != default) {
                AddChild(new ObjectRect(new ObjectRectConfig {
                    Texture = TextureHelper.Create(texture, TextureType.GameAtlas),
                    X = 39,
                    Y = RowHeight / 2,
                    Width = 50,
                    Height = 50,
                    Anchor = UiAnchor.Middle,
                    OutlineEnabled = false,
                    GlowEnabled = false
                }));
            }
        }

        AddChild(new SimpleText(new TextConfig {
            Text = name,
            FontSize = 20,
            FontType = FontType.Bold,
            Color = 0xFFFFFF,
            X = 79,
            Y = RowHeight / 2,
            MaxWidth = width - 190,
            Anchor = UiAnchor.MiddleLeft
        }));

        if (locked) {
            AddChild(new SimpleText(new TextConfig {
                Text = "Locked",
                FontSize = 16,
                Color = 0xD0D0D0,
                X = width - 18,
                Y = RowHeight / 2,
                Anchor = UiAnchor.MiddleRight
            }));
        }

        _selectionMarker = new Container(new ContainerConfig {
            X = width - 36,
            Y = RowHeight / 2,
            Width = 30,
            Height = 30,
            Anchor = UiAnchor.Middle
        });

        _selectionMarker.AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = 30,
            Height = 30,
            CutX = 4,
            CutY = 4,
            Color = 0xFFFFFF
        }));

        _selectionInterior = new CutEdgeRect(new CutEdgeConfig {
            X = 2,
            Y = 2,
            Width = 26,
            Height = 26,
            CutX = 3,
            CutY = 3,
            Color = BackgroundColor
        });

        _selectionMarker.AddChild(_selectionInterior);
        _selectionFill = new CutEdgeRect(new CutEdgeConfig {
            X = 6,
            Y = 6,
            Width = 18,
            Height = 18,
            CutX = 3,
            CutY = 3,
            Color = 0xFFFFFF
        });

        _selectionFill.Visible = false;
        _selectionMarker.AddChild(_selectionFill);
        _selectionMarker.Visible = !locked;
        AddChild(_selectionMarker);

        MouseEnabled = true;
        AddEventListener(MouseEvent.MouseOver, OnMouseOver);
        AddEventListener(MouseEvent.MouseOut, OnMouseOut);
        AddEventListener(MouseEvent.LeftClick, () => {
            if (!_locked) {
                onSelected(this);
            }
        });
    }

    public void SetSelected(bool selected) {
        _selected = selected;
        _selectionFill.Visible = selected;
        _background.SetColor(selected ? SelectedColor : BackgroundColor);
        _selectionInterior.SetColor(selected ? SelectedColor : BackgroundColor);
    }

    private void OnMouseOver() {
        if (!_locked && !_selected) {
            _background.SetColor(HoverColor);
            _selectionInterior.SetColor(HoverColor);
        }
    }

    private void OnMouseOut() {
        _background.SetColor(_selected ? SelectedColor : BackgroundColor);
        _selectionInterior.SetColor(_selected ? SelectedColor : BackgroundColor);
    }
}