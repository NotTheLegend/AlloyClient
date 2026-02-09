using System;
using AlloyClient.Assets;
using AlloyClient.Rendering.VertexData;
using Common.Structs;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.Types;

public sealed class TypeWallTop : RenderBase {
    
    private float _sortId;

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
        Position.X = x - 0.5f; // fixme: move the 0.5 to vertex data 
        Position.Y = y - 0.5f;
        Position.Z = z;
    }

    public override void SetTexture(AtlasData texture, bool attackFrame) {
        UV = texture.ToVector4(true);
    }

    public override void SetVisibility(bool visible) {
        Visible = visible;
    }

    public override void SetDepth(float depth) {
        _sortId = depth;
    }

    public override void SetAlpha(float alpha) {
        throw new NotSupportedException("Walls do not support alpha");
    }
    
    public override void SetName(string name) { }

    public override void Draw() {
        Render.DrawModel(new VertexModel(Position, UV, new Vector3(0, _sortId, RenderConfig.NoShade)));
    }
}