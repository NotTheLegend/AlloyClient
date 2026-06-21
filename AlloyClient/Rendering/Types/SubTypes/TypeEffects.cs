using System;
using System.Collections.Generic;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.VertexData;
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
    
    public override unsafe void Draw(float yOffset, List<VertexObject> targets, double time) {
        Scale.W = -0.5f;

        var total = Entity.EffectBuckets.TotalIcons;
        if (total < 1) {
            return;
        }

        Span<Vector4> effects = stackalloc Vector4[total];
        Entity.EffectBuckets.GetEffectsData(effects, (int)(time / 500));
        
        var pos = Parent.Position;
        var num = (total - 1) * Size;
        
        var s = MathF.Sin(-Settings.CameraAngle);
        var c = MathF.Cos(-Settings.CameraAngle);

        for (var i = 0; i < total; i++) {
            var x = num * c - yOffset * s;
            var y = num * s + yOffset * c;

            var p = new Vector3(pos.X - x, pos.Y + y, pos.Z);
            
            targets.Add(new VertexObject(p, effects[i], Scale, Rotation, Extra, Color));
            num -= Size * 2;
        }
    }
}