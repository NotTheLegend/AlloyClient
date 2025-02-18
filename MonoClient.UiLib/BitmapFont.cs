using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Common.Atlas;
using Common.Pipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoClient.UiLib;

public class BitmapFamily {

    public readonly Texture2D Atlas;
    
    public readonly Dictionary<FontType, BitmapFont> Fonts = [];

    public readonly float PixelRange;

    public BitmapFamily(string fontFamily) {
        var data = UiRender.Content.Load<FontFamily>(fontFamily);

        Atlas = data.Atlas;

        foreach (var kvp in data.FontData) {
            Fonts[kvp.Key] = new BitmapFont(kvp.Value, Atlas.Width);
        }

        PixelRange = data.PixelRange;
    }

}

public class BitmapFont {

    public readonly float LineHeight;
    public readonly float Ascender;
    public readonly float Descender;
    public readonly float OutlineTexel;

    public readonly Dictionary<char, FontGlyph> Glyphs;
    public readonly Dictionary<(char, char), float> Kernings;

    public BitmapFont(FontData fontData, int atlasWidth) {
        LineHeight = fontData.LineHeight;
        Ascender = fontData.Ascender;
        Descender = fontData.Descender;
        Glyphs = fontData.Glyphs;
        Kernings = fontData.Kernings;
        OutlineTexel = atlasWidth;// ??? tf was i on when i made this, i dont this is right for whatever its used for
    }

    //todo do this better
    public float ValidateOutlineSize(float size) {
        return 2 * Math.Max(Math.Min(size, OutlineTexel / 2f), 0f);
    }

    public (int, int) GetStartIndex(StringBuilder text, int caretIndex, int maxWidth, float outlineSize, float scale) {
        if (text.Length < 1 || maxWidth < 1)
            return (0, 0);

        var index = text.Length - 1;
        var startIndex = 0;
        var endIndex = text.Length;
        var width = 0f;

        while (index >= 0) {
            switch (text[index]) {
                case '\n':
                case '\r':
                    break;
                default:
                    if (!Glyphs.TryGetValue(text[index], out var glyph))
                        break;

                    if (index > 0) {
                        Kernings.TryGetValue((text[index - 1], text[index]), out var kern);
                        width += kern * scale;
                    }

                    width += glyph.Advance * scale;
                    width += outlineSize / OutlineTexel * scale * 2;
                    break;
            }

            if (width > maxWidth) {
                startIndex = index + 1;
                break;
            }

            index--;
        }
        
        return (startIndex, endIndex);
    }
    
    public int GetStartIndex(string text, int maxWidth, float outlineSize, float scale) {
        if (string.IsNullOrWhiteSpace(text) || maxWidth < 1)
            return 0;

        var index = text.Length - 1;
        var startIndex = 0;
        var width = 0f;

        while (index >= 0) {
            switch (text[index]) {
                case '\n':
                case '\r':
                    break;
                default:
                    if (!Glyphs.TryGetValue(text[index], out var glyph))
                        break;

                    if (index > 0) {
                        Kernings.TryGetValue((text[index - 1], text[index]), out var kern);
                        width += kern * scale;
                    }

                    width += glyph.Advance * scale;
                    width += outlineSize / OutlineTexel * scale * 2;
                    break;
            }

            if (width >= maxWidth) {
                startIndex = index + 1;
                break;
            }

            index--;
        }

        return startIndex;
    }
    
    public int GetCharCount(StringBuilder text) {
        var count = 0;

        for (var i = 0; i < text.Length; i++) {
            var c = text[i];
            switch (c) {
                case '\n':
                case '\r':
                    continue;
                default:
                    if (!Glyphs.TryGetValue(c, out _))
                        break;
                    count++;
                    continue;
            }
        }

        return count;
    }

    public int GetCharCount(string text) {
        var count = 0;
        foreach (var c in text) {
            switch (c) {
                case '\n':
                case '\r':
                    continue;
                default:
                    if (!Glyphs.ContainsKey(c)) {
                        break;
                    }
                    
                    count++;
                    continue;
            }
        }

        return count;
    }
}