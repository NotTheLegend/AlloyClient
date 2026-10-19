using Alloy.Common;
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
    public Color Color = Color.White;
    public Color OutlineColor = Color.Black;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.Default;

    public TextConfig() { }
}

public sealed class SimpleText : Sprite {

    private enum RenderType : int {
        Normal = 0,
        Small = 1
    }
    
    public string Text { get; private set; }
    
    private float _fontScale;
    private float _outlineThickness;
    private float _lineWrapStart;
    private readonly int _maxWidth;
    private readonly BitmapFont _font;
    private Color _color;
    private Color _outlineColor;

    public SimpleText(TextConfig config) {
        Text = config.Text;
        _fontScale = config.FontSize;
        _font = UiRender.GetFont(config.FontType);
        _maxWidth = config.MaxWidth;
        X = config.X;
        Y = config.Y;
        Alpha = config.Alpha;
        _outlineThickness = _font.ValidateOutlineSize(config.OutlineThickness);
        _color = config.Color;
        _outlineColor = config.OutlineColor;
        Anchor = config.Anchor;

        TextureId = TextureType.Text;
        
        Rebuild();
    }

    private void FillData() {
        var data = GetGraphicsBuffer(_font.GetCharCount(Text) * 6);
        var scale = _fontScale;
        var zero = new Vector2(0f, _font.Ascender * scale);
        var lastSpaceIndex = 0;
        var boundWidth = 0f;
        var boundHeight = _font.Descender * scale;
        var len = Text.Length;
        var idx = 0;
        for (var i = 0; i < len; i++) {
            var c = Text[i];
            if (c == ' ') { // Track last space
                lastSpaceIndex = i;
            }
            
            switch (c) {
                case '\n':
                    if (zero.X > boundWidth)
                        boundWidth = zero.X;
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
                    data[idx * 6 + 0] = new VertexUi(new Vector2(zero.X + pos.X0 * scale, zero.Y - pos.Y0 * scale), new Vector2(uv.X0, uv.Y0), _color); //tl
                    data[idx * 6 + 1] = new VertexUi(new Vector2(zero.X + pos.X1 * scale, zero.Y - pos.Y0 * scale), new Vector2(uv.X1, uv.Y0), _color); //tr
                    data[idx * 6 + 2] = new VertexUi(new Vector2(zero.X + pos.X1 * scale, zero.Y - pos.Y1 * scale), new Vector2(uv.X1, uv.Y1), _color); //br
                    data[idx * 6 + 3] = new VertexUi(new Vector2(zero.X + pos.X0 * scale, zero.Y - pos.Y0 * scale), new Vector2(uv.X0, uv.Y0), _color); //tl
                    data[idx * 6 + 4] = new VertexUi(new Vector2(zero.X + pos.X1 * scale, zero.Y - pos.Y1 * scale), new Vector2(uv.X1, uv.Y1), _color); //br
                    data[idx * 6 + 5] = new VertexUi(new Vector2(zero.X + pos.X0 * scale, zero.Y - pos.Y1 * scale), new Vector2(uv.X0, uv.Y1), _color); //bl
                    idx++;

                    if (i < len - 1) {
                        _font.Kernings.TryGetValue((c, Text[i + 1]), out var kern);
                        zero.X += kern * scale;
                    }

                    zero.X += glyph.Advance * scale;
                    break;
            }
            
            //todo add param for wordwrap
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
                continue;
            }
        }

        if (zero.X > boundWidth) {
            boundWidth = zero.X;
        }
        
        SetGraphicsBuffer();
        
        Extra1.X = _outlineThickness;
        Extra1.Y = (int) (_fontScale < 16 ? RenderType.Small : RenderType.Normal);
    }

    private void Rebuild() {
        FillData();
        SetColorChannel1(_color);
        SetColorChannel2(_outlineColor);
    }

    public void SetText(string text) {
        if (text == Text) {
            return;
        }
        
        Text = text;
        Rebuild();
    }

    public void SetColor(Color color) => SetColorChannel1(_color = color);

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