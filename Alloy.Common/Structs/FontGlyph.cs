using System.Text.Json;
using OpenTK.Mathematics;

namespace Alloy.Common.Structs;

public struct FontGlyph {
    public char Character;
    public float Advance;

    public GlyphData Position;
    public GlyphData UV;

    public FontGlyph(char character, float advance, GlyphData position, GlyphData uv) {
        Character = character;
        Advance = advance;
        Position = position;
        UV = uv;
    }
}

public struct GlyphData {
    public float X0;
    public float X1;
    public float Y0;
    public float Y1;

    public static GlyphData FromJson(JsonElement element) {
        return new GlyphData {
            X0 = element.GetProperty("left").GetSingle(),
            X1 = element.GetProperty("right").GetSingle(),
            Y0 = element.GetProperty("top").GetSingle() * -1,
            Y1 = element.GetProperty("bottom").GetSingle() * -1
        };
    }

    public static GlyphData FromJson(JsonElement element, float width, float height) {
        return new GlyphData {
            X0 = element.GetProperty("left").GetSingle() / width,
            X1 = element.GetProperty("right").GetSingle() / width,
            Y0 = element.GetProperty("top").GetSingle() / height,
            Y1 = element.GetProperty("bottom").GetSingle() / height
        };
    }

    public Vector4 ToVector4() {
        return new Vector4(X0, Y0, X1 - X0, Y1 - Y0);
    }
}