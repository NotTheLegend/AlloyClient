using System;
using System.Runtime.InteropServices;
using AlloyClient.Engine.Common;
using AlloyClient.Engine.Graphics.Buffers;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ShadowData(Vector2 position, float scale, Color color) : IBufferData<ShadowData> {
    public Vector2 Position = position;
    public float Scale = scale;
    public uint Color = color.PackedValue;

    public override int GetHashCode() {
        HashCode.Combine(Position, Scale, Color);
        return (Position.GetHashCode() * 397 ^ Scale.GetHashCode()) * 397 ^ Color.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Scale + " TextureCoordinate:" + Color + "}}";
    }

    public static bool operator ==(ShadowData left, ShadowData right) {
        return left.Position == right.Position && left.Scale.Equals(right.Scale) && left.Color == right.Color;
    }

    public static bool operator !=(ShadowData left, ShadowData right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (ShadowData)obj;
    }

    public bool Equals(ShadowData other) {
        return Position.Equals(other.Position) && Scale.Equals(other.Scale) && Color.Equals(other.Color);
    }
}