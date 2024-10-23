using Common.Atlas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Common.Pipeline;

public class MainAtlas {
    public Texture2D Texture;
    public Dictionary<string, AtlasData[]> AtlasMapStatic;
    public Dictionary<string, AnimationAtlasData[]> AtlasMapAnimation;
    public Dictionary<string, Color[]> DominantColors;

    public AtlasData GetAtlasData(string lookup, int index) {
        if (lookup != null) {
            var b = AtlasMapStatic.TryGetValue(lookup, out var list);
            if (b && list.Length >= 1 && list.Length > index) {
                return list[index];
            }
        }

        Console.WriteLine($"Unable to lookup game atlas: {lookup} - {index}");
        return new AtlasData();

    }

    public AnimationAtlasData GetAnimationAtlasData(string lookup, int index) {
        if (lookup != null) {
            var b = AtlasMapAnimation.TryGetValue(lookup, out var list);
            if (b && list.Length >= 1 && list.Length > index) {
                return list[index];
            }
        }

        Console.WriteLine($"Unable to lookup game atlas[AnimationAtlasData]: {lookup} - {index}");
        return new AnimationAtlasData();
    }

    public Color GetDominantColor(string lookup, int index) {
        if (lookup != null) {
            var b = DominantColors.TryGetValue(lookup, out var list);
            if (b && list.Length >= 1 && list.Length > index) {
                return list[index];
            }
        }

        Console.WriteLine($"Unable to lookup game atlas[DominantColor]: {lookup} - {index}");
        return Color.Black;
    }
}

public class MainAtlasReader : ContentTypeReader<MainAtlas> {
    protected override MainAtlas Read(ContentReader reader, MainAtlas existingInstance) {
        var atlas = new MainAtlas();

        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var imgData = reader.ReadBytes(reader.ReadInt32());

        var readerObj = reader.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceManager));

        if (readerObj == null) {
            throw new Exception("GraphicsDeviceManager is null.");
        }

        var graphicsDeviceManager = (GraphicsDeviceManager) readerObj;
        var graphicsDevice = graphicsDeviceManager.GraphicsDevice;
        var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
        texture.SetData(imgData);
        atlas.Texture = texture;

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

        atlas.AtlasMapStatic = atlasMapStatic;

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

        atlas.AtlasMapAnimation = atlasMapAnimation;

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

        atlas.DominantColors = dominantColors;

        return atlas;
    }
}