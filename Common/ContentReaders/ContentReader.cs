using Alloy.Engine.Graphics;

namespace Common.ContentReaders;

public static class ContentReader {

    private static string _folder;

    public static void Init(string folder) {
        _folder = folder;
    }
    
    public static Texture LoadTexture(string imagePath) => new Texture(Path.Combine(_folder, imagePath));

    public static Atlas LoadAtlas(string path) {
        using var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(Path.Combine(_folder, path))));
        return Atlas.Read(reader);
    }

    public static FontFamily LoadFont(string path) {
        using var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(Path.Combine(_folder, path))));
        return FontFamily.Read(reader);
    }

    public static Shader LoadShader(string path, (string, string)[] defines = null) {
        return new Shader(Path.Combine(_folder, path), defines);
    }

}