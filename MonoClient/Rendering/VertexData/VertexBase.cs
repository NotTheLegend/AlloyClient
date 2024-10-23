using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBase(Vector3 position, Vector2 textureCoordinate) : IVertexType, IEquatable<VertexBase> {
    public Vector3 Position = position;
    public Vector2 UV = textureCoordinate;

    public static readonly VertexDeclaration VertexDeclaration = new([
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.BlendWeight, 0)
    ]);

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public override int GetHashCode() {
        return Position.GetHashCode() * 397 ^ UV.GetHashCode();
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