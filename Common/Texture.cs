using OpenTK.Graphics.OpenGL;
using ReFuel.Stb;

namespace Common;

public sealed class Texture {

    public readonly int Handle;
    
    public readonly int Width;
    
    public readonly int Height;
    
    public Texture(int handle, int width, int height) {
        Handle = handle;
        Width = width;
        Height = height;
    }

    public static Texture FromStream(Stream stream) {
        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, handle);

        using var img = StbImage.Load(stream, StbiImageFormat.Rgba);
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.ImagePointer);
        return new Texture(handle, img.Width, img.Height);
    }

    public static Texture FromFile(string file) {
        return FromStream(File.Open(file, FileMode.Open));
    }

}