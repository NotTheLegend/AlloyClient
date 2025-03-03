using System;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Assets;
using MonoClient.Objects;
using MonoClient.Rendering.Types.SubTypes;
using MonoClient.Rendering.VertexData;
using MonoClient.Utils;

namespace MonoClient.Rendering.Types;

public sealed class TypePlayer : RenderBase {
    
    public override ModelType ModelType {
        get => ModelType.PbObject;
    }

    public override bool HasShadow {
        get => true;
    }
    
    private readonly Player _player;
    
    private AtlasData _texture;
    
    private TypeName _typeName;
    private readonly TypeHpBar _hpBar;
    private readonly TypeBar _mpBar;

    public TypePlayer(Player player) {
        Entity = player;
        _player = player;
        
        SetTexture(player.GetTexture());
        Extra = new ExtraData(RenderConfig.TypeGameObject, RenderConfig.Shade);
        
        _typeName = new TypeName(this, player);
        _hpBar = new TypeHpBar(this, player);
        _mpBar = new TypeBar(this, player, ColorUtils.ColorHex(0x6084E0));
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
        _typeName.SetDepth(depth);
        _hpBar.SetDepth(depth);
        _mpBar.SetDepth(depth);
    }
    
    public override void SetAlpha(float alpha) {
        Extra.Alpha = alpha;
        _typeName.SetAlpha(alpha);
        _hpBar.SetAlpha(alpha);
        _mpBar.SetAlpha(alpha);
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
        
        Entity.HeightOffset = -1 * Scale.Y * k + Scale.W * k;
        
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
        
    }

    public override void DrawShadow() {
        if (Entity.Size == 0) return;
        Render.DrawShadow(new VertexShadow(Position, new Vector2(0.5f, 0.25f), Color.Black));
    }
}