using System;
using System.Runtime.InteropServices;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace RealmClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TileData(Vector4 posOffset, Vector4 uv, Vector4 animate, Vector4 blendLeftRight, Vector4 blendTopBottom, Vector4 cornerBottom, Vector4 cornerTop) : IBufferData, IEquatable<TileData> {

    public Vector4 Position = posOffset;
    public Vector4 UV = uv;
    public Vector4 Animate = animate;
    public Vector4 BlendLeftRight = blendLeftRight;
    public Vector4 BlendTopBottom = blendTopBottom;
    public Vector4 CornerBottom = cornerBottom;
    public Vector4 CornerTop = cornerTop;
    
    public static unsafe int Size { get; } = sizeof(TileData);

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

    public static bool operator ==(TileData left, TileData right) {
        return left.UV == right.UV &&
               left.Position == right.Position &&
               left.Animate == right.Animate &&
               left.BlendLeftRight == right.BlendLeftRight &&
               left.BlendTopBottom == right.BlendTopBottom &&
               left.CornerBottom == right.CornerBottom &&
               left.CornerTop == right.CornerTop;
    }

    public static bool operator !=(TileData left, TileData right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (TileData)obj;
    }

    public bool Equals(TileData other) {
        return Position.Equals(other.Position) && UV.Equals(other.UV) && Animate.Equals(other.Animate) && 
               BlendLeftRight.Equals(other.BlendLeftRight) && BlendTopBottom.Equals(other.BlendTopBottom) && 
               CornerBottom.Equals(other.CornerBottom) && CornerTop.Equals(other.CornerTop);
    }
}