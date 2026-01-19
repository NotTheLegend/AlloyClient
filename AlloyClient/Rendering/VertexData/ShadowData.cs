using System.Runtime.InteropServices;
using AlloyClient.Engine.Common;
using Common.Rendering;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ShadowData(Vector3 position, Vector2 scale, Color color) : IBufferData<ShadowData> {
    public Vector4 Position = new (position, 0);
    public Vector2 Scale = scale;
    public uint Color = color.PackedValue;
    public float _;

    public static unsafe int Size { get; } = sizeof(ShadowData);

    public override int GetHashCode() {
        return (Position.GetHashCode() * 397 ^ Scale.GetHashCode()) * 397 ^ Color.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Scale + " TextureCoordinate:" + Color + "}}";
    }

    public static bool operator ==(ShadowData left, ShadowData right) {
        return left.Position == right.Position && left.Scale == right.Scale && left.Color == right.Color;
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