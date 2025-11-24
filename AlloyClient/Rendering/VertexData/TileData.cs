using System;
using System.Runtime.InteropServices;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TileData(Vector4 posOffset, Vector4 uv, Vector4 animate, Vector4 mask) : IBufferData<TileData> {

    public Vector4 Position = posOffset;
    public Vector4 UV = uv;
    public Vector4 Animate = animate;
    public Vector4 Mask = mask;
    
    public static unsafe int Size { get; } = sizeof(TileData);

    public override int GetHashCode() {
        return ((Position.GetHashCode() * 397 ^ UV.GetHashCode())
                            * 397 ^ Animate.GetHashCode())
                    * 397 ^ Mask.GetHashCode();
    }

    public override string ToString() {
        return "{{TextureCoordinate:" + UV + "}}";
    }

    public static bool operator ==(TileData left, TileData right) {
        return left.UV == right.UV &&
               left.Position == right.Position &&
               left.Animate == right.Animate &&
               left.Mask == right.Mask;
    }

    public static bool operator !=(TileData left, TileData right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (TileData)obj;
    }

    public bool Equals(TileData other) {
        return Position.Equals(other.Position) && UV.Equals(other.UV) && Animate.Equals(other.Animate) && Mask.Equals(other.Mask);
    }
}