using System;
using System.Runtime.InteropServices;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MonoClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexParticle(Vector3 position, Vector4 color) : IVertexData, IEquatable<VertexParticle> {
    public Vector3 Position = position;
    public Vector4 Color = color;
    
    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(2, VertexAttribPointerType.Float, FormatType.Vector3),
        new ElementFormat(3, VertexAttribPointerType.Float, FormatType.Vector4)
    ], true);

    public override int GetHashCode() {
        return Position.GetHashCode() * 397 ^ Color.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Position + " TextureCoordinate:" + Color + "}}";
    }

    public static bool operator ==(VertexParticle left, VertexParticle right) {
        return left.Position == right.Position && left.Color == right.Color;
    }

    public static bool operator !=(VertexParticle left, VertexParticle right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexParticle)obj;
    }

    public bool Equals(VertexParticle other) {
        return Position.Equals(other.Position) && Color.Equals(other.Color);
    }
}