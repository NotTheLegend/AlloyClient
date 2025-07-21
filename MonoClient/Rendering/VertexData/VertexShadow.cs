using System;
using System.Runtime.InteropServices;
using Common;
using OpenTK.Mathematics;

namespace MonoClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexShadow(Vector3 position, Vector2 scale, Color color) : IVertexType, IEquatable<VertexShadow> {
    public Vector3 Position = position;
    public Vector2 Scale = scale;
    public Color Color = color;

    public static readonly VertexDeclaration VertexDeclaration = new([
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.TextureCoordinate, 2)
    ]);

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

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