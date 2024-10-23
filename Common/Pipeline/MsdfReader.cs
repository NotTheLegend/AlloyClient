using System.Text.Json;
using Common.Atlas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Common.Pipeline;

public class MsdfData {
    public readonly Texture2D Texture;

    public readonly float LineHeight;
    public readonly float PixelRange;
    public readonly float Ascender;
    public readonly float Descender;

    public readonly Dictionary<char, FontGlyph> Glyphs;
    public readonly Dictionary<(char, char), float> Kernings;

    public MsdfData(Texture2D texture, float lineHeight, float pixelRange, float ascender, float descender,
        Dictionary<char, FontGlyph> glyphs, Dictionary<(char, char), float> kernings) {
        Texture = texture;
        LineHeight = lineHeight;
        PixelRange = pixelRange;
        Ascender = ascender;
        Descender = descender;
        Glyphs = glyphs;
        Kernings = kernings;
    }
}

public class MsdfReader : ContentTypeReader<MsdfData> {
    protected override MsdfData Read(ContentReader reader, MsdfData existingInstance) {
        var readerObj = reader.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceManager));
        if (readerObj == null) throw new Exception("GraphicsDeviceManager is null.");
        var graphicsDevice = ((GraphicsDeviceManager)readerObj).GraphicsDevice;

        var png = reader.ReadBytes(reader.ReadInt32());
        using var stream = new MemoryStream(png);
        var texture = Texture2D.FromStream(graphicsDevice, stream, null);


        var jdoc = JsonDocument.Parse(reader.ReadString());

        var lineHeight = jdoc.RootElement.GetProperty("metrics").GetProperty("lineHeight").GetSingle();
        var ascender = jdoc.RootElement.GetProperty("metrics").GetProperty("ascender").GetSingle() * -1;
        var descender = jdoc.RootElement.GetProperty("metrics").GetProperty("descender").GetSingle() * -1;

        var jAtlas = jdoc.RootElement.GetProperty("atlas");
        var range = jAtlas.GetProperty("distanceRange").GetSingle();
        var width = jAtlas.GetProperty("width").GetSingle();
        var height = jAtlas.GetProperty("height").GetSingle();

        var jGlyphs = jdoc.RootElement.GetProperty("glyphs");
        var glyphs = new Dictionary<char, FontGlyph>(jGlyphs.GetArrayLength());

        foreach (var glyphElement in jGlyphs.EnumerateArray()) {
            var c = (char)glyphElement.GetProperty("unicode").GetInt32();
            var adv = glyphElement.GetProperty("advance").GetSingle();

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

        var jKerning = jdoc.RootElement.GetProperty("kerning");
        var kernings = new Dictionary<(char, char), float>(jKerning.GetArrayLength());

        foreach (var kernElement in jKerning.EnumerateArray()) {
            var c1 = (char)kernElement.GetProperty("unicode1").GetInt32();
            var c2 = (char)kernElement.GetProperty("unicode2").GetInt32();
            var kernAdv = kernElement.GetProperty("advance").GetSingle();
            kernings.Add((c1, c2), kernAdv);
        }

        return new MsdfData(texture, lineHeight, range, ascender, descender, glyphs, kernings);
    }
}