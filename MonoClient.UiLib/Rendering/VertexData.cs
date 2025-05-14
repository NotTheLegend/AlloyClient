using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.UiLib.Extra;

namespace MonoClient.UiLib.Rendering;

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
internal struct VertexDataUi : IVertexType, IEquatable<VertexDataUi> {
    public Vector2 Position;
    public Color Color;
    public Color ColorOverride;
    public Vector2 Info;
    public Vector2 UVCoords;
    public Vector4 Scissor;
    public Vector4 Extra1;
    public Vector4 Extra2;
    public Vector4 ColorTransform;

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
        new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 1),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
        new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
        new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
        new VertexElement(80, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5));

    public VertexDataUi(Vector2 position, Color color, Color colorOverride, Vector2 info, Vector2 uvCoords, Vector4 scissor, Vector4 extra1, Vector4 extra2, ColorTransform colorTransform) {
        Position = position;
        Color = color;
        ColorOverride = colorOverride;
        Info = info;
        UVCoords = uvCoords;
        Scissor = scissor;
        Extra1 = extra1;
        Extra2 = extra2;
        ColorTransform = colorTransform.GetTransformData();
    }

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

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