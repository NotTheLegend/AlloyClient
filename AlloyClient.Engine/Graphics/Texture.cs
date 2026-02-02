using AlloyClient.Engine.Common;
using ReFuel.Stb;

namespace AlloyClient.Engine.Graphics;

public sealed class Texture {
    
    public readonly int Width;
    
    public readonly int Height;
    
    internal uint TextureUnit;
    
    internal readonly int Handle;

    public Texture(string file) : this(File.ReadAllBytes(file)) { }

    public Texture(ReadOnlySpan<byte> data) : this(StbImage.Load(data, StbiImageFormat.Rgba)) { }
    
    public Texture(StbImage image) : this(image.AsSpan<Color>(), image.Width, image.Height) { }
    
    public Texture(ReadOnlySpan<Color> data, int width, int height) {
        Width = width;
        Height = height;
        Handle = GL.CreateTexture(TextureTarget.Texture2d);
        
        GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgba8, width, height);
        GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        
        SetFilter(TextureFilter.Nearest);
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) => GL.TextureSubImage2D(Handle, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);

    public void SetData(ReadOnlySpan<Color> data, int width, int height) => GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    
    public void SetFilter(TextureFilter filter) {
        GL.TextureParameteri(Handle, TextureParameterName.TextureMagFilter, filter.MagFilter);
        GL.TextureParameteri(Handle, TextureParameterName.TextureMinFilter, filter.MinFilter);
    }

    public void BindToTextureUnit(uint unit) {
        if (unit > 15) {
            throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
        }
        
        GL.BindTextureUnit(TextureUnit = unit, Handle);
    }
}