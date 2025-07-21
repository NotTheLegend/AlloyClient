using System;
using System.Runtime.InteropServices;
using Common;
using OpenTK.Mathematics;

namespace MonoClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexObject(Vector3 position, Vector4 uv, Vector4 scale, Vector4 rotation, Vector4 extra, Color color) : IVertexType, IEquatable<VertexObject> {
    public Vector3 Position = position;
    public Vector4 UV = uv;
    public Vector4 Scale = scale;
    public Vector4 Rotation = rotation;
    public Vector4 Extra = extra;
    public Color Color = color;

    public static readonly VertexDeclaration VertexDeclaration = new([
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(28, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
        new VertexElement(44, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
        new VertexElement(60, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
        new VertexElement(76, VertexElementFormat.Color, VertexElementUsage.Color, 0),
    ]);

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public override int GetHashCode() {
        return ((((Position.GetHashCode() * 397 ^ UV.GetHashCode())
                        * 397 ^ Scale.GetHashCode())
                    * 397 ^ Rotation.GetHashCode())
                * 397 ^ Extra.GetHashCode())
            * 397 ^ Color.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Position + " TextureCoordinate:" + UV + "}}";
    }

    public static bool operator ==(VertexObject left, VertexObject right) {
        return left.Position == right.Position &&
               left.UV == right.UV &&
               left.Scale == right.Scale &&
               left.Rotation == right.Rotation &&
               left.Extra == right.Extra &&
               left.Color == right.Color;
    }

    public static bool operator !=(VertexObject left, VertexObject right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexObject)obj;
    }

    public bool Equals(VertexObject other) {
        return Position.Equals(other.Position) && UV.Equals(other.UV) && Scale.Equals(other.Scale) && Rotation.Equals(other.Rotation) && Extra.Equals(other.Extra) && Color.Equals(other.Color);
    }
}