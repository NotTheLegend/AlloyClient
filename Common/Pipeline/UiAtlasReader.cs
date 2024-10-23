using Common.Atlas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Common.Pipeline;

public class UiAtlas {
    public Texture2D Texture;
    public Dictionary<string, AtlasData[]> AtlasMapFull = new();

    public AtlasData GetAtlasData(string lookup, int index = 0) {
        var b = AtlasMapFull.TryGetValue(lookup, out var list);
        if (!b || list.Length < 1) {
            Console.WriteLine($"Unable to lookup ui atlas: {lookup} - {index}");
        }
        
        return list![index];
    }
}

public class UiAtlasReader : ContentTypeReader<UiAtlas> {
    protected override UiAtlas Read(ContentReader reader, UiAtlas existingInstance) {
        var atlas = new UiAtlas();

        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var imgData = reader.ReadBytes(reader.ReadInt32());

        var readerObj = reader.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceManager));

        if (readerObj == null) {
            throw new Exception("GraphicsDeviceManager is null.");
        }

        var graphicsDeviceManager = (GraphicsDeviceManager)readerObj;
        var graphicsDevice = graphicsDeviceManager.GraphicsDevice;
        var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
        texture.SetData(imgData);
        atlas.Texture = texture;

        var mapFull = new Dictionary<string, AtlasData[]>();
        var fullCount = reader.ReadInt32();

        for (var i = 0; i < fullCount; i++) {
            var key = reader.ReadString();
            var value = new AtlasData[reader.ReadInt32()];

            for (var j = 0; j < value.Length; j++) {
                value[j] = reader.ReadAtlasData();
            }

            mapFull[key] = value;
        }

        atlas.AtlasMapFull = mapFull;

        return atlas;
    }
}