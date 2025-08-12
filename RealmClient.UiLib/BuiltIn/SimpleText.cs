using OpenTK.Mathematics;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Data;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Rendering;

namespace RealmClient.UiLib.BuiltIn;

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
    private float _firstLineOffset;
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
    
    protected override void ResizeBackBuffer() {
        var size = _font.GetCharCount(Text);
        VertexData = new VertexUi[size * 4];
        Indices = new short[size * 6];
        for (var i = 0; i < Indices.Length / 6; i++) {
            var idx6 = i * 6;
            var idx4 = i * 4;

            Indices[idx6] = (short)(0 + idx4);
            Indices[idx6 + 1] = (short)(1 + idx4);
            Indices[idx6 + 2] = (short)(2 + idx4);
            Indices[idx6 + 3] = (short)(0 + idx4);
            Indices[idx6 + 4] = (short)(2 + idx4);
            Indices[idx6 + 5] = (short)(3 + idx4);
        }
        base.ResizeBackBuffer();
    }

    private void FillData() {
        var scale = _fontScale;
        var zero = new Vector2(_firstLineOffset, _font.Ascender * scale);
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
                    zero.X = 0f;
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
                    VertexData[idx * 4 + 0] = new VertexUi(new Vector2(zero.X + pos.X0 * scale, zero.Y - pos.Y1 * scale), new Vector2(uv.X0, uv.Y1)); //bl
                    VertexData[idx * 4 + 1] = new VertexUi(new Vector2(zero.X + pos.X0 * scale, zero.Y - pos.Y0 * scale), new Vector2(uv.X0, uv.Y0)); //tl
                    VertexData[idx * 4 + 2] = new VertexUi(new Vector2(zero.X + pos.X1 * scale, zero.Y - pos.Y0 * scale), new Vector2(uv.X1, uv.Y0)); //tr
                    VertexData[idx * 4 + 3] = new VertexUi(new Vector2(zero.X + pos.X1 * scale, zero.Y - pos.Y1 * scale), new Vector2(uv.X1, uv.Y1)); //br
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
                
                zero.X = 0f;
                boundHeight += _font.LineHeight * scale;
                zero.Y += _font.LineHeight * scale;
                continue;
            }
        }

        if (zero.X > boundWidth) {
            boundWidth = zero.X;
        }
        
        SetBaseDimensions((int)boundWidth, (int)(boundHeight + _font.LineHeight * scale));
        Extra1.X = _outlineThickness;
        Extra1.Y = (int) (_fontScale < 16 ? RenderType.Small : RenderType.Normal);
    }

    private void Rebuild() {
        var size = _font.GetCharCount(Text);
        if (size * 4 <= VertexData.Length) {
            OverridePrimCount = size * 2;
        }
        else {
            ResizeBackBuffer();
        }
        
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

    public void OffsetFirstLineBy(float value) {
        _firstLineOffset = value;
    }
}