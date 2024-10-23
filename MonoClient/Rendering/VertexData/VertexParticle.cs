using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexParticle(Vector3 position, Vector4 color) : IVertexType, IEquatable<VertexParticle> {
    public Vector3 Position = position;
    public Vector4 Color = color;

    public static readonly VertexDeclaration VertexDeclaration = new([
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
    ]);

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

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