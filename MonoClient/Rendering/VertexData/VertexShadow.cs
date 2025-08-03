using System;
using System.Runtime.InteropServices;
using Common;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MonoClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexShadow(Vector3 position, Vector2 scale, Color color) : IVertexData, IEquatable<VertexShadow> {
    public Vector3 Position = position;
    public Vector2 Scale = scale;
    public uint Color = color.PackedValue;
    
    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(2, VertexAttribPointerType.Float, FormatType.Vector3),
        new ElementFormat(3, VertexAttribPointerType.Float, FormatType.Vector2),
        new ElementFormat(4, VertexAttribPointerType.UnsignedInt, FormatType.Color),
    ], true);

    public override int GetHashCode() {
        return (Position.GetHashCode() * 397 ^ Scale.GetHashCode()) * 397 ^ Color.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Scale + " TextureCoordinate:" + Color + "}}";
    }

    public static bool operator ==(VertexShadow left, VertexShadow right) {
        return left.Position == right.Position && left.Scale == right.Scale && left.Color == right.Color;
    }

    public static bool operator !=(VertexShadow left, VertexShadow right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexShadow)obj;
    }

    public bool Equals(VertexShadow other) {
        return Position.Equals(other.Position) && Scale.Equals(other.Scale) && Color.Equals(other.Color);
    }
}