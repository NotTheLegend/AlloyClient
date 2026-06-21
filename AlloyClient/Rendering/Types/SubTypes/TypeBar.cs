using System.Collections.Generic;
using Alloy.Common;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.Types.SubTypes;

public class TypeBar : SubRenderBase {

    public override float Height {
        get => 0.12f * 2;
    }

    private Color _bgColor = Color.FromHexRGB(0x111111);
    private Vector4 _bgScale = new Vector4(0.72f, 0.12f, 0, 0);

    public TypeBar(RenderBase parent, Entity entity, Color color) {
        Parent = parent;
        Entity = entity;
        Color = color;

        UV = new Vector4();
        Scale = new Vector4(0.68f, 0.08f, 0, 0);
        Rotation = new Vector4(0, 1, 1, -1);
        Extra = new ExtraData(RenderConfig.TypeBar, RenderConfig.NoShade);
    }

    public void SetFill(float percent) {
        if (percent < 0f) return;
        
        Scale.Z = 0.68f * percent - 0.68f;
        Scale.X = 0.68f * percent;
    }
    
    public override void Draw(float yOffset, List<VertexObject> targets, double time) {
        _bgScale.W = yOffset;
        Scale.W = yOffset;
        targets.Add(new VertexObject(Parent.Position, UV, Scale, Rotation, Extra, Color));
        targets.Add(new VertexObject(Parent.Position, UV, _bgScale, Rotation, Extra/* + new Vector4(0, 0.001f, 0, 0)*/, _bgColor)); // TODO: make bar outlines ddx/ddy instead of 2nd quad
    }
}