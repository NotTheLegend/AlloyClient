using Common.Atlas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common.ContentReaders;

public class Atlas {
    
    private readonly Texture2D _texture;
    private readonly Dictionary<string, AtlasData[]> _atlasMapStatic;
    private readonly Dictionary<string, AnimationAtlasData[]> _atlasMapAnimation;
    private readonly Dictionary<string, Color[]> _dominantColors;

    private Atlas(Texture2D texture, Dictionary<string, AtlasData[]> atlasMapStatic, Dictionary<string, AnimationAtlasData[]> atlasMapAnimation, Dictionary<string, Color[]> dominantColors) {
        _texture = texture;
        _atlasMapStatic = atlasMapStatic;
        _atlasMapAnimation = atlasMapAnimation;
        _dominantColors = dominantColors;
    }

    public Texture2D GetTexture() => _texture;
    
    public AtlasData GetAtlasData(string lookup, int index) {
        if (lookup != null) {
            var b = _atlasMapStatic.TryGetValue(lookup, out var list);
            if (b && list.Length >= 1 && list.Length > index) {
                return list[index];
            }
        }

        Console.WriteLine($"Unable to lookup atlas: {lookup} - {index}");
        return new AtlasData();

    }

    public AnimationAtlasData GetAnimationAtlasData(string lookup, int index) {
        if (lookup != null) {
            var b = _atlasMapAnimation.TryGetValue(lookup, out var list);
            if (b && list.Length >= 1 && list.Length > index) {
                return list[index];
            }
        }

        Console.WriteLine($"Unable to lookup atlas[AnimationAtlasData]: {lookup} - {index}");
        return new AnimationAtlasData();
    }

    public Color GetDominantColor(string lookup, int index) {
        if (lookup != null) {
            var b = _dominantColors.TryGetValue(lookup, out var list);
            if (b && list.Length >= 1 && list.Length > index) {
                return list[index];
            }
        }

        Console.WriteLine($"Unable to lookup atlas[DominantColor]: {lookup} - {index}");
        return Color.Black;
    }

    internal static Atlas Read(BinaryReader reader, GraphicsDevice graphics) {
        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var imgData = reader.ReadBytes(reader.ReadInt32());
        
        var texture = new Texture2D(graphics, width, height, false, SurfaceFormat.Color);
        texture.SetData(imgData);

        var atlasMapStatic = new Dictionary<string, AtlasData[]>();
        var atlasDataMapCount = reader.ReadInt32();

        for (var i = 0; i < atlasDataMapCount; i++) {
            var key = reader.ReadString();
            var value = new AtlasData[reader.ReadInt32()];

            for (var j = 0; j < value.Length; j++) {
                value[j] = reader.ReadAtlasData();
            }

            atlasMapStatic[key] = value;
        }

        var atlasMapAnimation = new Dictionary<string, AnimationAtlasData[]>();
        var animationMapCount = reader.ReadInt32();

        for (var i = 0; i < animationMapCount; i++) {
            var key = reader.ReadString();
            var animData = new AnimationAtlasData[reader.ReadInt32()];

            for (var j = 0; j < animData.Length; j++) {
                var data = new AnimationAtlasData();
                var right = new AtlasData[reader.ReadInt32()];

                for (var k = 0; k < right.Length; k++) {
                    right[k] = reader.ReadAtlasData();
                }

                data.FaceRight = right;

                var down = new AtlasData[reader.ReadInt32()];

                for (var k = 0; k < down.Length; k++) {
                    down[k] = reader.ReadAtlasData();
                }

                data.FaceDown = down;

                var up = new AtlasData[reader.ReadInt32()];

                for (var k = 0; k < up.Length; k++) {
                    up[k] = reader.ReadAtlasData();
                }

                data.FaceUp = up;

                animData[j] = data;
            }

            atlasMapAnimation[key] = animData;
        }

        var dominantColors = new Dictionary<string, Color[]>();
        var dominantColorsCount = reader.ReadInt32();

        for (var i = 0; i < dominantColorsCount; i++) {
            var key = reader.ReadString();
            var value = new Color[reader.ReadInt32()];

            for (var j = 0; j < value.Length; j++) {
                value[j] = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            }

            dominantColors[key] = value;
        }

        return new Atlas(texture, atlasMapStatic, atlasMapAnimation, dominantColors);
    }
}