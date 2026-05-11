using System;
using System.Runtime.InteropServices;
using Alloy.Engine.Common;
using Alloy.Engine.Graphics.Buffers;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.VertexData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexObject(Vector3 position, Vector4 uv, Vector4 scale, Vector4 rotation, Vector4 extra, Color color) : IBufferData<VertexObject>, IComparable<VertexObject> {
    public Vector4 Position = new (position, 1);
    public Vector4 UV = uv;
    public Vector4 Scale = scale;
    public Vector4 Rotation = rotation;
    public Vector4 Extra = extra;
    public Vector4 Color = color.ToVector4();
    public Vector4 Mask1;
    public Vector4 Mask2;

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
    
    public int CompareTo(VertexObject other) {
        if (Extra.Y < other.Extra.Y) {
            return 1;
        }

        if (Extra.Y > other.Extra.Y) {
            return -1;
        }
        
        return 0;
    }
}