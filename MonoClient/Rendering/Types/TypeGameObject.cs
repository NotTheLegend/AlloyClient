using System;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Assets;
using MonoClient.Rendering.Types.SubTypes;
using MonoClient.Rendering.VertexData;
using MonoClient.Objects;

namespace MonoClient.Rendering.Types;

public sealed class TypeGameObject : RenderBase {
    
    public override ModelType ModelType {
        get => ModelType.PbObject;
    }

    public override bool HasShadow {
        get => true;
    }
    
    private AtlasData _texture;

    private readonly TypeName _name;

    private readonly TypeHpBar _hpBar;

    public TypeGameObject(Entity entity) {
        Entity = entity;
        SetTexture(entity.GetTexture());
        Extra = new ExtraData(RenderConfig.TypeGameObject, RenderConfig.Shade);
        _name = new TypeName(this, entity);
        _hpBar = new TypeHpBar(this, entity);
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
        _name.SetDepth(depth);
    }
    
    public override void SetAlpha(float alpha) {
        Extra.Alpha = alpha;
        _name.SetAlpha(alpha);
    }

    private float _t = 1f;

    public override void Draw() {
        var s = MathF.Sin(-Entity.Rotation);
        var c = MathF.Cos(-Entity.Rotation);
        var k = Entity.Size / 100f;
        var f = Entity.Flipped ? 1f : -1f;
        Rotation = new Vector4(s, c, k, f);
        
        //Extra.Alpha = 0.5f;

        var pos1 = Vector3.Transform(Position, Camera.SpeechMatrix);
        var pos2 = Vector3.Transform(new Vector3(Position.X, Position.Y - 1 * Scale.Y * k + Scale.W * k, Position.Z), Camera.SpeechMatrix);
        
        Entity.SpeechOffset = Vector3.Distance(pos1, pos2);
        
        
        Render.DrawEntity(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
        
        if (Entity.Properties.Static) return;
        
        var y = 0.1f;

        if (Entity.MaxHp != 0) {
            _hpBar.SetFill(_t -= 0.00001f);
            _hpBar.Draw(y);
        }
        
        
    }

    public override void DrawShadow() {
        if (Entity.Size == 0) return;
        Render.DrawShadow(new VertexShadow(Position, new Vector2(0.5f, 0.25f), Color.Black));
    }
}