using System;
using System.Runtime.InteropServices;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace RealmClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBase(Vector3 position, Vector2 textureCoordinate) : IVertexData, IEquatable<VertexBase> {
    public Vector3 Position = position;
    public Vector2 UV = textureCoordinate;

    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(0, VertexAttribPointerType.Float, FormatType.Vector3),
        new ElementFormat(1, VertexAttribPointerType.Float, FormatType.Vector2)
    ]);

    public override int GetHashCode() {
        return HashCode.Combine(Position, UV);
    }

    public override string ToString() {
        return "{{Position:" + Position + " TextureCoordinate:" + UV + "}}";
    }

    public static bool operator ==(VertexBase left, VertexBase right) {
        return left.Position == right.Position && left.UV == right.UV;
    }

    public static bool operator !=(VertexBase left, VertexBase right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexBase)obj;
    }

    public bool Equals(VertexBase other) {
        return Position.Equals(other.Position) && UV.Equals(other.UV);
    }
}