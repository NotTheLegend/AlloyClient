using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace MonoClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexTile(Vector4 posOffset, Vector4 uv, Vector4 animate, Vector4 blendLeftRight, Vector4 blendTopBottom, Vector4 cornerBottom, Vector4 cornerTop) : IVertexType, IEquatable<VertexTile> {

    public Vector4 Position = posOffset;
    public Vector4 UV = uv;
    public Vector4 Animate = animate;
    public Vector4 BlendLeftRight = blendLeftRight;
    public Vector4 BlendTopBottom = blendTopBottom;
    public Vector4 CornerBottom = cornerBottom;
    public Vector4 CornerTop = cornerTop;

    public static readonly VertexDeclaration VertexDeclaration = new([
        new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
        new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
        new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
        new VertexElement(80, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5),
        new VertexElement(96, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 6)
    ]);

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public override int GetHashCode() {
        return (((((Position.GetHashCode() * 397 ^ UV.GetHashCode())
                            * 397 ^ Animate.GetHashCode())
                        * 397 ^ BlendLeftRight.GetHashCode())
                    * 397 ^ BlendTopBottom.GetHashCode())
                * 397 ^ CornerBottom.GetHashCode())
            * 397 ^ CornerTop.GetHashCode();
    }

    public override string ToString() {
        return "{{TextureCoordinate:" + UV + "}}";
    }

    public static bool operator ==(VertexTile left, VertexTile right) {
        return left.UV == right.UV &&
               left.Position == right.Position &&
               left.Animate == right.Animate &&
               left.BlendLeftRight == right.BlendLeftRight &&
               left.BlendTopBottom == right.BlendTopBottom &&
               left.CornerBottom == right.CornerBottom &&
               left.CornerTop == right.CornerTop;
    }

    public static bool operator !=(VertexTile left, VertexTile right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexTile)obj;
    }

    public bool Equals(VertexTile other) {
        return Position.Equals(other.Position) && UV.Equals(other.UV) && Animate.Equals(other.Animate) && 
               BlendLeftRight.Equals(other.BlendLeftRight) && BlendTopBottom.Equals(other.BlendTopBottom) && 
               CornerBottom.Equals(other.CornerBottom) && CornerTop.Equals(other.CornerTop);
    }
}