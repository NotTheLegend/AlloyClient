using AlloyClient.Assets;
using AlloyClient.Rendering.VertexData;
using Common.Structs;

namespace AlloyClient.Rendering.Types;

public sealed class TypeWallTop : RenderBase {

    public override ModelType ModelType {
        get => ModelType.PbTile;
    }

    public override bool HasShadow {
        get => false;
    }

    public TypeWallTop(RenderBase renderBaseType) {
        Entity = renderBaseType.Entity;
        Extra = new ExtraData(RenderConfig.TypeWall, RenderConfig.NoShade);
    }
    
    public override void SetPosition(float x, float y, float z = 0) {
        Position.X = x;
        Position.Y = y;
        Position.Z = z;
    }

    public override void SetTexture(AtlasData texture, bool attackFrame) {
        UV = texture.ToVector4(true);
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
        Render.DrawEntity(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
    }
}