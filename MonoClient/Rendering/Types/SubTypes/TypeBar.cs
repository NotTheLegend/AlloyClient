using Microsoft.Xna.Framework;
using MonoClient.Rendering.VertexData;
using MonoClient.Objects;
using MonoClient.Utils;

namespace MonoClient.Rendering.Types.SubTypes;

public class TypeBar : SubRenderBase {

    public override float Height {
        get => 0.06f * 2;
    }

    private Color _bgColor = ColorUtils.ColorHex(0x111111);
    private Vector4 _bgScale = new Vector4(0.36f, 0.06f, 0, 0);

    public TypeBar(RenderBase parent, Entity entity, Color color) {
        Parent = parent;
        Entity = entity;
        Color = color;

        UV = new Vector4();
        Scale = new Vector4(0.34f, 0.04f, 0, 0);
        Rotation = new Vector4(0, 1, 1, -1);
        Extra = new ExtraData(RenderConfig.TypeBar, RenderConfig.NoShade);
    }

    public void SetFill(float percent) {
        if (percent < 0f) return;
        
        Scale.Z = 0.34f * percent - 0.34f;
        Scale.X = 0.34f * percent;
    }
    
    public override void Draw(float yOffset) {
        _bgScale.W = yOffset;
        Scale.W = yOffset;
        Render.DrawEntity(new VertexObject(Parent.Position, UV, _bgScale, Rotation, Extra.Data, _bgColor));
        Render.DrawEntity(new VertexObject(Parent.Position, UV, Scale, Rotation, Extra.Data, Color));
    }
}