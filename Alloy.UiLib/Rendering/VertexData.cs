using System;
using System.Runtime.InteropServices;
using Alloy.Common;
using Alloy.Engine.Graphics.Buffers;
using Alloy.UiLib.Extra;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Rendering;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SpriteInstanceData(SpriteVertexMatrix data, Vector2 info, Vector4 scissor, Vector4 extra1, Vector4 extra2, ColorTransform colorTransform) : IBufferData<SpriteInstanceData> {

    // Vertex Changes
    public Vector2 VertexScale = data.Scale;
    public Vector2 VertexRotation = new(data.Rotation, 0f);
    public Vector2 VertexOffset = data.Offset;
    public Vector2 VertexAnchor = data.Anchor;
    
    // Sprite Data
    public uint Padding;
    public uint Padding1;
    public Vector2 Info = info;
    public Vector4 Scissor = scissor;
    public Vector4 Extra1 = extra1;
    public Vector4 Extra2 = extra2;
    public Vector4 ColorTransform = colorTransform;

    public bool Equals(SpriteInstanceData other) {
        return Info.Equals(other.Info) && 
               Scissor.Equals(other.Scissor) && 
               Extra1.Equals(other.Extra1) && 
               Extra2.Equals(other.Extra2) && 
               ColorTransform.Equals(other.ColorTransform);
    }

    public override bool Equals(object obj) {
        return obj is SpriteInstanceData other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Info, Scissor, Extra1, Extra2, ColorTransform);
    }
}

internal readonly record struct SpriteVertexMatrix(Vector2 Scale, float Rotation, Vector2 Offset, Vector2 Anchor);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexUi : IVertexData<VertexUi> {
    
    public Vector2 Position;
    public Vector2 Uv;
    public Color ColorChannel1 = new Color(0);
    public Color ColorChannel2 = new Color(0);
    internal uint InstanceId;
    
    internal static VertexStride VertexStride { get; } = new([
        new ElementFormat(0, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(1, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(2, VertexAttribType.UnsignedInt, FormatType.Default),
        new ElementFormat(3, VertexAttribType.UnsignedInt, FormatType.Default),
        new ElementFormat(4, VertexAttribType.UnsignedInt, FormatType.Default),
    ]);
    
    public (Vector2, Vector2) PositionUv { set { Position = value.Item1; Uv = value.Item2; } }

    public VertexUi(Vector2 pos, Vector2 uv, Color colorChannel1) {
        Position = pos;
        Uv = uv;
        ColorChannel1 = colorChannel1;
    }
    
    public VertexUi(Vector2 pos, Vector2 uv) {
        Position = pos;
        Uv = uv;
    }
    
    public VertexUi(Vector2 pos, Color colorC1) {
        Position = pos;
        Uv = new Vector2(0f);
        ColorChannel1 = colorC1;
    }

    public VertexUi(Vector2 pos) {
        Position = pos;
        Uv = new Vector2(0f);
    }
    
    public bool Equals(VertexUi other) {
        return Position == other.Position && 
               Uv.Equals(other.Uv) && 
               ColorChannel1.Equals(other.ColorChannel1) && 
               ColorChannel2.Equals(other.ColorChannel2) && 
               InstanceId == other.InstanceId;
    }

    public override bool Equals(object obj) {
        return obj is VertexUi other && Equals(other);
    }
    
    
    public override int GetHashCode() {
        return HashCode.Combine(Position, Uv, ColorChannel1, ColorChannel2, InstanceId);
    }

    public static bool operator ==(VertexUi left, VertexUi right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexUi left, VertexUi right)
    {
        return !(left == right);
    }
}