using System;
using Common.Structs;
using OpenTK.Mathematics;
using RealmClient.Assets;
using RealmClient.Objects;
using RealmClient.Rendering.VertexData;
using RealmClient.Utils;

namespace RealmClient.Rendering.Types;

public sealed class TypeModel3D : RenderBase {
    private static readonly Logger Log = new(typeof(TypeModel3D));
    
    public override ModelType ModelType { get; }

    public override bool HasShadow {
        get => false;
    }

    public TypeModel3D(string modelName, Entity entity) {
        ModelType type;
        try {
            type = (ModelType) Enum.Parse(typeof(ModelType), modelName.Replace(" ", ""));
        }
        catch {
            Log.Error($"Failed to parse model type: {modelName}");
            return;
        }
        
        ModelType = type;
        Entity = entity;
        SetTexture(entity.GetTexture());

        Rotation.X = MathHelper.DegreesToRadians(entity.Properties.Rotation);
        Extra = new ExtraData(RenderConfig.TypeModel, RenderConfig.NoShade);
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