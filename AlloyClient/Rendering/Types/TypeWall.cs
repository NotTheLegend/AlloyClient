using Common.Structs;
using RealmClient.Assets;
using RealmClient.Assets.Libraries;
using RealmClient.Game.Objects;
using RealmClient.Rendering.VertexData;
using RealmClient.Utils;

namespace RealmClient.Rendering.Types;

public sealed class TypeWall : RenderBase {
    private static readonly Logger Log = new(typeof(TypeWall));

    public override ModelType ModelType { get; }

    public override bool HasShadow => false;

    public readonly TypeWallTop Top;
    private readonly int _topZ;
    
    public TypeWall(Entity entity, ModelType modelType = ModelType.PbWall) {
        Entity = entity;
        ModelType = modelType;

        _topZ = modelType switch {
            ModelType.PbWall => 1,
            ModelType.PbDoubleWall => 2,
            ModelType.PbDoubleWall2 => 2,
            ModelType.PbTripleWall => 3,
            _ => 1
        };
        
        var textureData = ObjectLibrary.TypeToTextureData[entity.Properties.ObjectType];
        SetTexture(textureData.GetTexture());

        Extra = new ExtraData(RenderConfig.TypeWall, RenderConfig.Shade);
        
        Top = new TypeWallTop(this);
        Top.SetTexture(textureData.GetTopTexture());
    }

    public override void SetPosition(float x, float y, float z = 0) {
        Position.X = x;
        Position.Y = y;
        Position.Z = z;
        Top.SetPosition(x, y, _topZ + z);
    }
    
    public override void SetVisibility(bool visible) {
        Visible = visible;
        Top.SetVisibility(visible);
    }
    
    public override void SetDepth(float depth) {
        Extra.SortId = depth;
        Top.SetDepth(depth);
    }

    public override void SetAlpha(float alpha) {
        Extra.Alpha = alpha;
        Top.SetAlpha(alpha);
    }

    public override void SetName(string name) { }

    public override void SetTexture(AtlasData texture, bool attackFrame) {
        UV = texture.ToVector4(true);
    }

    public override void Draw() {
        Render.DrawEntity(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
    }
}