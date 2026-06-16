using System.Collections.Generic;
using AlloyClient.Assets;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.VertexData;
using Alloy.Common.Structs;

namespace AlloyClient.Rendering.Types;

public sealed class TypeGroundObject : RenderBase {

    public override ModelType ModelType {
        get => ModelType.PbTile;
    }

    public override bool HasShadow {
        get => false;
    }

    public TypeGroundObject(Entity entity) {
        Entity = entity;
        Extra = new ExtraData(RenderConfig.TypeModel, RenderConfig.NoShade);
        
        SetTexture(entity.GetTexture());
        Extra.SortId = 1f;
    }
    
    public override void SetPosition(float x, float y, float z = 0) {
        Position.X = (int)x;
        Position.Y = (int)y;
    }

    public override void SetTexture(AtlasData texture, bool attackFrame) {
        UV = texture.ToVector4();
    }
    
    public override void SetVisibility(bool visible) {
        Visible = visible;
    }

    public override void SetDepth(float depth) { }
    
    public override void SetAlpha(float alpha) {
        Extra.Alpha = alpha;
    }
    
    public override void SetName(string name) { }

    public override void Draw(List<VertexObject> targets, double time) {
        targets.Add(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
    }
}