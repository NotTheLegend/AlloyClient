using System;
using System.Runtime.InteropServices;
using AlloyClient.Engine.Graphics.Buffers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexModel(Vector3 position, Vector4 uv, Vector3 extra) : IVertexData<VertexModel> {
    public Vector3 Position = position;
    public Vector4 UV = uv;
    public Vector3 Extra = extra;
    
    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(2, VertexAttribType.Float, FormatType.Vector3),
        new ElementFormat(3, VertexAttribType.Float, FormatType.Vector4),
        new ElementFormat(4, VertexAttribType.Float, FormatType.Vector3)
    ], true);

    public override int GetHashCode() => HashCode.Combine(Position, UV, Extra);

    public override string ToString() {
        return "{{Position:" + Position + " TextureCoordinate:" + UV + "}}";
    }

    public static bool operator ==(VertexModel left, VertexModel right) {
        return left.Position == right.Position &&
               left.UV == right.UV &&
               left.Extra == right.Extra;
    }

    public static bool operator !=(VertexModel left, VertexModel right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexModel)obj;
    }

    public bool Equals(VertexModel other) {
        return Position.Equals(other.Position) && UV.Equals(other.UV) && Extra.Equals(other.Extra);
    }
}