using System;
using Common;
using RealmClient.Utils;
using OpenTK.Mathematics;
using RealmClient.Assets;
using RealmClient.Objects;
using RealmClient.Rendering.Types.SubTypes;
using RealmClient.Rendering.VertexData;

namespace RealmClient.Rendering.Types;

public sealed class TypePlayer : RenderBase {
    
    public override ModelType ModelType {
        get => ModelType.PbObject;
    }

    public override bool HasShadow {
        get => true;
    }
    
    private readonly Player _player;
    
    private TypeName _typeName;
    private readonly TypeHpBar _hpBar;
    private readonly TypeBar _mpBar;
    private readonly TypeEffects _effects;

    public TypePlayer(Player player) {
        Entity = player;
        _player = player;
        
        SetTexture(player.GetTexture());
        Extra = new ExtraData(RenderConfig.TypeGameObject, RenderConfig.Shade);
        
        _typeName = new TypeName(this, player);
        _hpBar = new TypeHpBar(this, player);
        _mpBar = new TypeBar(this, player, Color.FromHexRGB(0x6084E0));
        _effects = new TypeEffects(this, player);
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
        _typeName.SetDepth(depth);
        _hpBar.SetDepth(depth);
        _mpBar.SetDepth(depth);
        _effects.SetDepth(depth);
    }
    
    public override void SetAlpha(float alpha) {
        Extra.Alpha = alpha;
        _typeName.SetAlpha(alpha);
        _hpBar.SetAlpha(alpha);
        _mpBar.SetAlpha(alpha);
        _effects.SetAlpha(alpha);
    }

    public override void SetName(string name) {
        _typeName.Name = name;
        _typeName.SetTextures();
    }

    public override void Draw() {
        var s = MathF.Sin(-Entity.Rotation);
        var c = MathF.Cos(-Entity.Rotation);
        var k = Entity.Size / 100f;
        var f = Entity.Flipped ? 1f : -1f;
        Rotation = new Vector4(s, c, k, f);
        
        Entity.HeightOffset = -0.5f * Scale.Y * k + Scale.W * k;
        
        Render.DrawEntity(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
        var y = 0.1f;
        if (_player != Map.LocalPlayer) {
            _typeName.Draw(y);
            y += _typeName.Height;
        }
        
        _hpBar.SetFill(1f * _player.Hp / _player.MaxHp);
        _hpBar.Draw(y);
        y += _hpBar.Height;
        _mpBar.Draw(y);
        
        _effects.Draw(Entity.HeightOffset);
        
    }

    public override void DrawShadow() {
        if (Entity.Size == 0) return;
        Render.DrawShadow(new ShadowData(Position, new Vector2(1f, 0.5f), Color.Black));
    }
}