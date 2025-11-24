using System;
using AlloyClient.Game.Objects;
using AlloyClient.Game.Objects.Util;
using AlloyClient.Rendering.VertexData;
using AlloyClient.State;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.Types.SubTypes;

public class TypeEffects : SubRenderBase {

    public override float Height {
        get => 0.12f * 2;
    }

    private const float Size = 0.32f;
    private const float SizeDouble = Size * 2;

    public TypeEffects(RenderBase parent, Entity entity) {
        Parent = parent;
        Entity = entity;

        UV = new Vector4();
        Scale = new Vector4(Size, Size, 0, -0.5f);
        Rotation = new Vector4(0, 1, 1f, -1);
        Extra = new ExtraData(RenderConfig.TypeEffect, RenderConfig.Shade);
    }
    
    public override void Draw(float yOffset) {
        Scale.W = -0.5f;

        var effects = (uint)(Entity.ConditionEffects & ~ConditionEffectUtil.IconlessEffects);
        if (effects <= 0) return;

        var pos = Parent.Position;
        
        var num = (System.Numerics.BitOperations.PopCount(effects) - 1) * Size;
        
        var s = MathF.Sin(-Settings.CameraAngle);
        var c = MathF.Cos(-Settings.CameraAngle);

        for (var i = 1; i < 32; i++) {
            if ((effects & 1 << i) == 0) continue;
            
            var x = num * c - yOffset * s;
            var y = num * s + yOffset * c;

            var p = new Vector3(pos.X - x, pos.Y + y, pos.Z);
            
            Render.DrawEntity(new VertexObject(p, ConditionEffectUtil.EffectIcons[(ConditionEffects)(1 << i)][0], Scale, Rotation, Extra.Data, Color));
            num -= Size * 2;
        }
    }
}