using System;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Assets;
using MonoClient.Rendering.Types.SubTypes;
using MonoClient.Rendering.VertexData;
using MonoClient.Objects;

namespace MonoClient.Rendering.Types;

public sealed class TypeProjectile : RenderBase {
    
    public override ModelType ModelType {
        get => ModelType.PbObject;
    }

    public override bool HasShadow {
        get => true;
    }

    public Projectile Projectile;
    
    private AtlasData _texture;

    public TypeProjectile(Projectile proj) {
        Projectile = proj;
        SetTexture(proj.GetTexture());
        Extra = new ExtraData(RenderConfig.TypeGameObject, RenderConfig.Shade);
    }
    
    public override void SetPosition(float x, float y, float z = 0) {
        Position.X = x;
        Position.Y = y;
        Position.Z = z;
    }

    public override void SetTexture(AtlasData texture, bool attackFrame) {
        _texture = texture;
        UV = texture.ToVector4();

        var frameMult = attackFrame ? 2f : 1f;
        var w = 4096 * _texture.W - 2;
        var h = 4096 * _texture.H - 2;
        
        var ratio = w / h / frameMult;

        var hw = w / 2;
        var hh = h / 2;

        var widthScale = 0.85f; // Base Scale
        widthScale *= 0.1f + 0.1f * hw; // Padding + pixel count
        widthScale *= ratio;
        
        var heightScale = 0.85f; // Base Scale
        heightScale *= 0.1f + 0.1f * hh; // Padding + pixel count
        heightScale *= ratio;
        
        var padX = attackFrame ? 0.85f * 0.1f * hw / 2 * ratio : 0f;
        var padY = 0.85f * -0.1f * hh * ratio;

        Scale = new Vector4(widthScale, heightScale, padX, padY);
    }
    
    public override void SetVisibility(bool visible) {
        Visible = visible;
    }

    public override void SetDepth(float depth) {
        Extra.SortId = depth;
    }
    
    public override void SetAlpha(float alpha) {
        Extra.Alpha = alpha;
    }

    public override void SetName(string name) { }

    public override void Draw() {
        var s = MathF.Sin(-Projectile.Rotation);
        var c = MathF.Cos(-Projectile.Rotation);
        var k = Projectile.Size / 100f;
        Rotation = new Vector4(s, c, k, -1f);
        
        Render.DrawEntity(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
    }

    public override void DrawShadow() {
        Render.DrawShadow(new VertexShadow(Position, new Vector2(0.5f, 0.25f), Color.Black));
    }
}