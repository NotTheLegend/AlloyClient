using Common;
using Microsoft.Xna.Framework;
using MonoClient.Rendering.VertexData;
using MonoClient.Objects;
using MonoClient.UiLib;

namespace MonoClient.Rendering.Types.SubTypes;

public class TypeName : SubRenderBase {

    private float _height;
    public override float Height {
        get => _height * 1.75f;
    }

    public string Name;

    private GlyphData[] _glyphs;

    public TypeName(RenderBase parent, Entity entity) {
        Parent = parent;
        Entity = entity;
        if (entity is Player player) {
            Name = player.Name;
        } else {
            Name = entity.Properties.DisplayName;
        }

        if (string.IsNullOrEmpty(Name))
            Name = "Default";
        
        SetTextures();
        Extra = new ExtraData(RenderConfig.TypeText, RenderConfig.NoShade);
    }

    public void SetTextures() {
        const float size = 0.5f * 0.5f;
        
        var font = UiRender.GetFont(FontType.Normal);
        _glyphs = new GlyphData[Name.Length];

        _height = font.Ascender * size;
        var zero = new Vector2(0f, _height);

        var len = Name.Length;
        for (var i = 0; i < len; i++) {
            var c = Name[i];
            if (!font.Glyphs.TryGetValue(c, out var glyph)) {
                continue;
            }

            var uv = glyph.UV;
            var pos = glyph.Position;
            var w = (pos.X1 - pos.X0) / 2.0f * size;
            var h = (pos.Y0 - pos.Y1) / 2.0f * size;

            _glyphs[i] = new GlyphData(uv.ToVector4(), w, h, zero.X, zero.Y - pos.Y0 * size);

            if (i < len - 1) {
                font.Kernings.TryGetValue((c, Name[i + 1]), out var kern);
                zero.X += kern * size * 2f;
            }

            zero.X += glyph.Advance * size;
            zero.X += 12f / font.OutlineTexel * size * 2f;
        }

        for (var index = 0; index < _glyphs.Length; index++) {
            _glyphs[index].Center(zero.X / 2f);
        }

        Rotation = new Vector4(0, 1, 1, -1);
        Color = new Color(0xFC, 0xDF, 0, 1);
    }
    
    public override void Draw(float yOffset) {
        for (var index = 0; index < _glyphs.Length; index++) {
            var g = _glyphs[index];
            g.Scale.W += yOffset;
            Render.DrawEntity(new VertexObject(Parent.Position, g.UV, g.Scale, Rotation, Extra.Data, Color));
        }
    }
    
    private struct GlyphData(Vector4 uv, float w, float h, float x, float y) {
        public Vector4 UV = uv;
        public Vector4 Scale = new(w, h, 1f * w + x, 1f * h + y);

        public void Center(float x) {
            Scale.Z -= x;
        }
    }
}