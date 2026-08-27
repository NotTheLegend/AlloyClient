using System;
using Alloy.UiLib.Core;
using Alloy.UiLib.Data;
using Alloy.UiLib.Rendering;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct TextConfig {
    public string Text = "";
    public float FontSize = 1f;
    public FontType FontType = FontType.Normal;
    public int X = 0;
    public int Y = 0;
    public int MaxWidth = -1;
    public float OutlineThickness = 0;
    public uint Color = 0xFFFFFF;
    public uint OutlineColor = 0x0;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public TextConfig() {
    }
}

public sealed class SimpleText : Sprite {

    private enum RenderType {
        Normal = 0,
        Small = 1,
    }

    public string Text;

    private float _fontScale;
    private float _outlineThickness;
    private float _lineWrapStart;
    private readonly int _maxWidth;
    private readonly BitmapFont _font;

    public SimpleText(TextConfig config) {
        Text = config.Text;
        _fontScale = config.FontSize;
        _font = UiRender.GetFont(config.FontType);
        _maxWidth = config.MaxWidth;
        X = config.X;
        Y = config.Y;
        Alpha = config.Alpha;
        _outlineThickness = _font.ValidateOutlineSize(config.OutlineThickness);
        SetColor(config.Color);
        SetColorSecondary(config.OutlineColor);
        SetAnchor(config.Anchor);

        TextureId = TextureType.Text;

        ResizeBackBuffer();
        FillData();
    }

    private void ResizeBackBuffer() {
        var size = _font.GetCharCount(Text);
        VertexData = new VertexUi[size * 4];
        Indices = new ushort[size * 6];
        for (var i = 0; i < Indices.Length / 6; i++) {
            var idx6 = i * 6;
            var idx4 = i * 4;

            Indices[idx6] = (ushort)(0 + idx4);
            Indices[idx6 + 1] = (ushort)(1 + idx4);
            Indices[idx6 + 2] = (ushort)(2 + idx4);
            Indices[idx6 + 3] = (ushort)(0 + idx4);
            Indices[idx6 + 4] = (ushort)(2 + idx4);
            Indices[idx6 + 5] = (ushort)(3 + idx4);
        }
    }

    private void FillData() {
        var scale = _fontScale;
        var zero = new Vector2(0f, _font.Ascender * scale);
        var lastSpaceIndex = 0;
        var boundWidth = 0f;
        var boundHeight = _font.Descender * scale;
        var len = Text.Length;
        var idx = 0;
        for (var i = 0; i < len; i++) {
            var c = Text[i];
            if (c == ' ') {
                // Track last space
                lastSpaceIndex = i;
            }

            switch (c) {
                case '\n':
                    if (zero.X > boundWidth) {
                        boundWidth = zero.X;
                    }

                    zero.X = _lineWrapStart;
                    boundHeight += _font.LineHeight * scale;
                    zero.Y += _font.LineHeight * scale;
                    continue;
                case '\r':
                    continue;
                default:
                    if (!_font.Glyphs.TryGetValue(c, out var glyph)) {
                        continue;
                    }

                    var uv = glyph.UV;
                    var pos = glyph.Position;
                    VertexData[idx * 4 + 0] = new VertexUi(new Vector2(zero.X + pos.X0 * scale, zero.Y - pos.Y1 * scale),
                        new Vector2(uv.X0, uv.Y1)); // Bottom left

                    VertexData[idx * 4 + 1] = new VertexUi(new Vector2(zero.X + pos.X0 * scale, zero.Y - pos.Y0 * scale),
                        new Vector2(uv.X0, uv.Y0)); // Top left

                    VertexData[idx * 4 + 2] = new VertexUi(new Vector2(zero.X + pos.X1 * scale, zero.Y - pos.Y0 * scale),
                        new Vector2(uv.X1, uv.Y0)); // Top right

                    VertexData[idx * 4 + 3] = new VertexUi(new Vector2(zero.X + pos.X1 * scale, zero.Y - pos.Y1 * scale),
                        new Vector2(uv.X1, uv.Y1)); // Bottom right

                    idx++;

                    if (i < len - 1) {
                        _font.Kernings.TryGetValue((c, Text[i + 1]), out var kern);
                        zero.X += kern * scale;
                    }

                    zero.X += glyph.Advance * scale;
                    break;
            }

            // Todo: add param for word wrap
            // Max width hit, start new line
            if (_maxWidth > -1 && zero.X >= _maxWidth) {
                // Prevent word being cut by the new line if there was
                if (lastSpaceIndex > 0) {
                    idx -= i - lastSpaceIndex;
                    i = lastSpaceIndex;
                    lastSpaceIndex = 0;
                }

                if (zero.X > boundWidth) {
                    boundWidth = zero.X;
                }

                zero.X = _lineWrapStart;
                boundHeight += _font.LineHeight * scale;
                zero.Y += _font.LineHeight * scale;
            }
        }

        if (zero.X > boundWidth) {
            boundWidth = zero.X;
        }

        SetGraphicsBuffer();

        Extra1.X = _outlineThickness;
        Extra1.Y = (int)(_fontScale < 16 ? RenderType.Small : RenderType.Normal);
    }

    private void Rebuild() {
        var size = _font.GetCharCount(Text);
        var activeVertexCount = size * 4;
        if (activeVertexCount > VertexData.Length) {
            ResizeBackBuffer();
        } else if (activeVertexCount < VertexData.Length) {
            Array.Clear(VertexData, activeVertexCount, VertexData.Length - activeVertexCount);
        }

        OverridePrimCount = size * 2;

        FillData();
    }

    public void SetText(string text) {
        if (text == Text) {
            return;
        }

        Text = text;
        Rebuild();
    }

    public void SetFontSize(float size) {
        if (size == _fontScale) {
            return;
        }

        _fontScale = size;
        Rebuild();
    }

    public void SetOutlineSize(float size) {
        if (size == _outlineThickness) {
            return;
        }

        _outlineThickness = _font.ValidateOutlineSize(size);
        Rebuild();
    }

    public void OffsetLineWrapBy(float x) {
        _lineWrapStart = x;
        Rebuild();
    }
}
