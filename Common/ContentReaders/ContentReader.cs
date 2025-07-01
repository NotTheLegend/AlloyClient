using Microsoft.Xna.Framework.Graphics;

namespace Common.ContentReaders;

public static class ContentReader {

    private static string _folder;

    private static GraphicsDevice _graphics;

    public static void Init(string folder, GraphicsDevice graphics) {
        _folder = folder;
        _graphics = graphics;
    }
    
    public static Texture2D LoadTexture(string imagePath) => Texture2D.FromFile(_graphics, Path.Combine(_folder, imagePath));

    public static Atlas LoadAtlas(string path) {
        using var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(Path.Combine(_folder, path))));
        return Atlas.Read(reader, _graphics);
    }

    public static FontFamily LoadFont(string path) {
        using var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(Path.Combine(_folder, path))));
        return FontFamily.Read(reader, _graphics);
    }

}