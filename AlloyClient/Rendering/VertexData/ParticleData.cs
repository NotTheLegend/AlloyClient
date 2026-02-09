using System.Runtime.InteropServices;
using AlloyClient.Engine.Graphics.Buffers;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ParticleData(Vector3 position, Vector4 color) : IBufferData<ParticleData> {
    public Vector4 Position = new Vector4(position, 1);
    public Vector4 Color = color;
    
    public static unsafe int Size { get; } = sizeof(ParticleData);

    public override int GetHashCode() {
        return Position.GetHashCode() * 397 ^ Color.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Position + " TextureCoordinate:" + Color + "}}";
    }

    public static bool operator ==(ParticleData left, ParticleData right) {
        return left.Position == right.Position && left.Color == right.Color;
    }

    public static bool operator !=(ParticleData left, ParticleData right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (ParticleData)obj;
    }

    public bool Equals(ParticleData other) {
        return Position.Equals(other.Position) && Color.Equals(other.Color);
    }
}