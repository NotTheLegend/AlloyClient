using Alloy.Common;
using Alloy.Engine.Graphics;

namespace Alloy.ContentReader;

public static class ContentLoader {

    private static string _folder;

    public static void Init(string folder) {
        _folder = folder;
    }
    
    public static Texture LoadTexture(string imagePath) => new Texture(Path.CombineAlt(_folder, imagePath));

    public static Atlas LoadAtlas(string path) {
        using var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(Path.CombineAlt(_folder, path))));
        return Atlas.Read(reader);
    }

    public static FontFamily LoadFont(string path) {
        using var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(Path.CombineAlt(_folder, path))));
        return FontFamily.Read(reader);
    }
}