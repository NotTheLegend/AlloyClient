using System.Buffers;
using System.Text.Json;
using AlloyClient.Engine.Graphics;
using Common.Structs;

namespace Common.ContentReaders;

public class FontFamily {

    public readonly Texture Texture;

    public readonly Dictionary<string, FontData> FontData;

    public float PixelRange;

    private FontFamily(Texture texture, Dictionary<string, FontData> fontData, float pixelRange) {
        Texture = texture;
        FontData = fontData;
        PixelRange = pixelRange;
    }

    internal static FontFamily Read(BinaryReader reader) {
        var length = reader.ReadInt32();
        var png = ArrayPool<byte>.Shared.Rent(length);
        reader.Read(png, 0, length);
        var texture = new Texture(png);
        ArrayPool<byte>.Shared.Return(png);
        
        var fontFamily = new Dictionary<string, FontData>();
        var fontOrder = new string[reader.ReadInt32()];
        for (var i = 0; i < fontOrder.Length; i++) {
            fontOrder[i] = reader.ReadString();
        }
        
        var jdoc = JsonDocument.Parse(reader.ReadString());
        
        var jAtlas = jdoc.RootElement.GetProperty("atlas");
        var pixelRange = jAtlas.GetValueFloat("distanceRange");
        var width = jAtlas.GetValueFloat("width");
        var height = jAtlas.GetValueFloat("height");

        var variants = jdoc.RootElement.GetProperty("variants");

        var idx = 0;
        foreach (var fontData in variants.EnumerateArray()) {
            var metrics = fontData.GetProperty("metrics");
            var lineHeight = metrics.GetValueFloat("lineHeight");
            var ascender = metrics.GetValueFloat("ascender") * -1;
            var descender = metrics.GetValueFloat("descender") * -1;
            
            var glyphData = fontData.GetProperty("glyphs");
            var glyphs = new Dictionary<char, FontGlyph>(glyphData.GetArrayLength());

            foreach (var glyphElement in glyphData.EnumerateArray()) {
                var c = (char)glyphElement.GetValueInt("unicode");
                var adv = glyphElement.GetValueFloat("advance");

                var pos = new GlyphData();

                if (glyphElement.TryGetProperty("planeBounds", out var planeBounds)) {
                    pos = GlyphData.FromJson(planeBounds);
                }

                var uv = new GlyphData();

                if (glyphElement.TryGetProperty("atlasBounds", out var atlasBounds)) {
                    uv = GlyphData.FromJson(atlasBounds, width, height);
                }

                glyphs.Add(c, new FontGlyph(c, adv, pos, uv));
            }

            var kerningData = fontData.GetProperty("kerning");
            var kernings = new Dictionary<(char, char), float>(kerningData.GetArrayLength());

            foreach (var kernElement in kerningData.EnumerateArray()) {
                var c1 = (char)kernElement.GetValueInt("unicode1");
                var c2 = (char)kernElement.GetValueInt("unicode2");
                var kernAdv = kernElement.GetValueFloat("advance");
                kernings.Add((c1, c2), kernAdv);
            }
            
            fontFamily[fontOrder[idx]] = new FontData(lineHeight, ascender, descender, glyphs, kernings);
            idx++;
        }

        return new FontFamily(texture, fontFamily, pixelRange);
    }
}

public sealed class FontData {

    public readonly float LineHeight;
    public readonly float Ascender;
    public readonly float Descender;

    public readonly Dictionary<char, FontGlyph> Glyphs;
    public readonly Dictionary<(char, char), float> Kernings;
    
    public FontData(float lineHeight, float ascender, float descender, Dictionary<char, FontGlyph> glyphs, Dictionary<(char, char), float> kernings) {
        LineHeight = lineHeight;
        Ascender = ascender;
        Descender = descender;
        Glyphs = glyphs;
        Kernings = kernings;
    }
}