using System;
using System.Runtime.InteropServices;
using Common;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RealmClient.UiLib.Extra;

namespace RealmClient.UiLib.Rendering;

public struct VertexUi {
    
    public Vector2 Position;
    public Vector2 UV;
    public Color Color;

    public VertexUi(Vector2 pos, Vector2 uv, Color color) {
        Position = pos;
        UV = uv;
        Color = color;
    }
    
    public VertexUi(Vector2 pos, Vector2 uv) {
        Position = pos;
        UV = uv;
        Color = new Color(0);
    }
    
    public VertexUi(Vector2 pos, Color color) {
        Position = pos;
        UV = new Vector2(0f);
        Color = color;
    }

    public VertexUi(Vector2 pos) {
        Position = pos;
        UV = new Vector2(0f);
        Color = new Color(0);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VertexDataUi : IVertexData, IEquatable<VertexDataUi> {
    public Vector2 Position;
    public uint Color;
    public uint ColorOverride;
    public Vector2 Info;
    public Vector2 UVCoords;
    public Vector4 Scissor;
    public Vector4 Extra1;
    public Vector4 Extra2;
    public Vector4 ColorTransform;

    public VertexDataUi(Vector2 position, Color color, Color colorOverride, Vector2 info, Vector2 uvCoords, Vector4 scissor, Vector4 extra1, Vector4 extra2, ColorTransform colorTransform) {
        Position = position;
        Color = color.PackedValue;
        ColorOverride = colorOverride.PackedValue;
        Info = info;
        UVCoords = uvCoords;
        Scissor = scissor;
        Extra1 = extra1;
        Extra2 = extra2;
        ColorTransform = colorTransform.GetTransformData();
    }
    
    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(0, VertexAttribPointerType.Float, FormatType.Vector2),
        new ElementFormat(1, VertexAttribPointerType.UnsignedInt, FormatType.Color),
        new ElementFormat(2, VertexAttribPointerType.UnsignedInt, FormatType.Color),
        new ElementFormat(3, VertexAttribPointerType.Float, FormatType.Vector2),
        new ElementFormat(4, VertexAttribPointerType.Float, FormatType.Vector2),
        new ElementFormat(5, VertexAttribPointerType.Float, FormatType.Vector4),
        new ElementFormat(6, VertexAttribPointerType.Float, FormatType.Vector4),
        new ElementFormat(7, VertexAttribPointerType.Float, FormatType.Vector4),
        new ElementFormat(8, VertexAttribPointerType.Float, FormatType.Vector4)
    ]);

    public override int GetHashCode() {
        return (((((((Position.GetHashCode() 
                                        * 397 ^ Color.GetHashCode())
                                    * 397 ^ ColorOverride.GetHashCode())
                                * 397 ^ Info.GetHashCode())
                            * 397 ^ UVCoords.GetHashCode())
                        * 397 ^ Scissor.GetHashCode())
                    * 397 ^ Extra1.GetHashCode())
                * 397 ^ Extra2.GetHashCode())
            * 397 ^ ColorTransform.GetHashCode();
    }

    public override string ToString() {
        return "{{Position:" + Position
                             + " Color: " + Color
                             + " Override: " + ColorOverride
                             + " Info: " + Info
                             + " UVCoords:" + UVCoords
                             + " Scissor:" + Scissor
                             + " E1:" + Extra1
                             + " E2:" + Extra2
                             + " CT:" + ColorTransform + "}}";
    }

    public static bool operator ==(VertexDataUi left, VertexDataUi right) {
        return left.Position == right.Position
               && left.Color == right.Color
               && left.ColorOverride == right.ColorOverride
               && left.Info == right.Info
               && left.UVCoords == right.UVCoords
               && left.Scissor == right.Scissor
               && left.Extra1 == right.Extra1
               && left.Extra2 == right.Extra2
               && left.ColorTransform == right.ColorTransform;
    }

    public static bool operator !=(VertexDataUi left, VertexDataUi right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return obj != null && !(obj.GetType() != GetType()) && this == (VertexDataUi)obj;
    }

    public bool Equals(VertexDataUi other) {
        return Position.Equals(other.Position) && 
               Color.Equals(other.Color) &&
               ColorOverride.Equals(other.ColorOverride) && 
               Info.Equals(other.Info) && 
               UVCoords.Equals(other.UVCoords) && 
               Scissor.Equals(other.Scissor) && 
               Extra1.Equals(other.Extra1) && 
               Extra2.Equals(other.Extra2) && 
               ColorTransform.Equals(other.ColorTransform);
    }
}