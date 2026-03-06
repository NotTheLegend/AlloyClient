using System;
using AlloyClient.Assets;
using AlloyClient.Engine.Common;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.Types;

public sealed class TypeProjectile : RenderBase {
    
    public override ModelType ModelType {
        get => ModelType.PbObject;
    }

    public override bool HasShadow {
        get => true;
    }

    public TypeProjectile() {
        Extra = new ExtraData(RenderConfig.TypeGameObject, RenderConfig.Shade);
    }
    
    public override void SetPosition(float x, float y, float z = 0) {
        Position.X = x;
        Position.Y = y;
        Position.Z = z;
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
        var s = MathF.Sin(-RotationAngle);
        var c = MathF.Cos(-RotationAngle);
        var k = Size / 100f;
        Rotation = new Vector4(s, c, k, -1f);
        
        Render.DrawEntity(new VertexObject(Position, UV, Scale, Rotation, Extra.Data, Color));
    }

    public override void DrawShadow() {
        Render.DrawShadow(new ShadowData(Position.Xy, 0.5f, Color.Black));
    }
}