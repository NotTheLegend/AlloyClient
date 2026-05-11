using System;
using System.Collections.Generic;
using Alloy.Engine.Common;
using AlloyClient.Assets;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.Types.SubTypes;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.Types;

public sealed class TypeGameObject : RenderBase {
    
    public override ModelType ModelType {
        get => ModelType.PbObject;
    }

    public override bool HasShadow {
        get => true;
    }

    private readonly TypeName _name;

    private readonly TypeHpBar _hpBar;
    private readonly TypeEffects _effects;

    public TypeGameObject(Entity entity) {
        Entity = entity;
        SetTexture(entity.GetTexture());
        Extra = new ExtraData(RenderConfig.TypeGameObject, RenderConfig.Shade);
        _name = new TypeName(this, entity);
        _hpBar = new TypeHpBar(this, entity);
        _effects = new TypeEffects(this, entity);

        Color = Color.Black;
    }
    
    public override void SetPosition(float x, float y, float z = 0) {
        Position.X = x;
        Position.Y = y;
        Position.Z = z;
    }
    
    public override void SetVisibility(bool visible) {
        Visible = visible;
    }

    public override void SetDepth(float depth) {
        Extra.SortId = depth;
        _name.SetDepth(depth);
        _hpBar.SetDepth(depth);
        _effects.SetDepth(depth);
    }
    
    public override void SetAlpha(float alpha) {
        Extra.Alpha = alpha;
        _name.SetAlpha(alpha);
        _hpBar.SetAlpha(alpha);
        _effects.SetAlpha(alpha);
    }

    public override void SetName(string name) { }

    public override void Draw(List<VertexObject> targets) {
        var s = MathF.Sin(-Entity.Rotation);
        var c = MathF.Cos(-Entity.Rotation);
        var k = Entity.Size / 100f;
        var f = Entity.Flipped ? 1f : -1f;
        Rotation = new Vector4(s, c, k, f);
        
        Entity.HeightOffset = -1 * Scale.Y * k + Scale.W * k;
        
        targets.Add(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
        
        if (Entity.Properties.Static) return;
        
        var y = 0.1f;

        if (Entity.MaxHp != 0) {
            _hpBar.SetFill(1f * Entity.Hp / Entity.MaxHp);
            _hpBar.Draw(y, targets);
        }
        
        _effects.Draw(Entity.HeightOffset, targets);
    }

    public override void DrawShadow() {
        if (Entity.Size == 0) return;
        Render.DrawShadow(new ShadowData(Position.Xy, 1f, Color.Black));
    }
}