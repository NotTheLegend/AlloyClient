using System;
using System.Collections.Generic;
using AlloyClient.Assets;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.VertexData;
using Alloy.Common.Structs;
using AlloyClient.Logging;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.Types;

public sealed class TypeModel3D : RenderBase {
    private readonly static ILogger Logger = ILogger.CreateLogger(nameof(TypeModel3D));
    
    public override ModelType ModelType { get; }

    public override bool HasShadow => false;

    private float _rotation;

    private float _sortId;

    public TypeModel3D(string modelName, Entity entity) {
        ModelType type;
        try {
            type = (ModelType) Enum.Parse(typeof(ModelType), modelName.Replace(" ", ""));
        } catch {
            Logger.Log(LogLevel.Error, $"Failed to parse model type: {modelName}");
            return;
        }

        ModelType = type;
        Entity = entity;
        SetTexture(entity.GetTexture());

        _rotation = MathHelper.DegreesToRadians(entity.Properties.Rotation);
        Extra = new ExtraData(RenderConfig.TypeModel, RenderConfig.Shade);
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
        _sortId = depth;
    }
    
    public override void SetAlpha(float alpha) {
        throw new NotSupportedException("Models do not support alpha");
    }
    
    public override void SetName(string name) { }

    public override void Draw(List<VertexObject> targets, double time) {
        Render.DrawModel(new VertexModel(Position, UV, new Vector3(_rotation, _sortId, RenderConfig.Shade)));
    }
}