using Alloy.Engine.Common;
using ReFuel.Stb;

namespace Alloy.Engine.Graphics;

public sealed class Texture {
    
    public readonly int Width;
    
    public readonly int Height;
    
    internal readonly int Handle;

    public Texture(string file) : this(File.ReadAllBytes(file)) { }

    public Texture(ReadOnlySpan<byte> data) : this(StbImage.Load(data, StbiImageFormat.Rgba)) { }
    
    public Texture(StbImage image) : this(image.AsSpan<Color>(), image.Width, image.Height) { }
    
    public Texture(ReadOnlySpan<Color> data, int width, int height) {
        Width = width;
        Height = height;
        Handle = GL.CreateTexture(TextureTarget.Texture2D);
        
        GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgba8, width, height);
        GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) => GL.TextureSubImage2D(Handle, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);

    public void SetData(ReadOnlySpan<Color> data, int width, int height) => GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
}