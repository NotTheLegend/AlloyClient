using AlloyClient.Engine.Common;
using ReFuel.Stb;

namespace AlloyClient.Engine.Graphics;

public sealed class Texture {
    
    public readonly int Width;
    
    public readonly int Height;
    
    internal uint TextureUnit;
    
    private readonly int _handle;

    public Texture(string file) : this(File.ReadAllBytes(file)) { }

    public Texture(ReadOnlySpan<byte> data) : this(StbImage.Load(data, StbiImageFormat.Rgba)) { }
    
    public Texture(StbImage image) : this(image.AsSpan<Color>(), image.Width, image.Height) { }
    
    public Texture(ReadOnlySpan<Color> data, int width, int height) {
        Width = width;
        Height = height;
        _handle = GL.CreateTexture(TextureTarget.Texture2d);
        
        GL.TextureStorage2D(_handle, 1, SizedInternalFormat.Rgba8, width, height);
        GL.TextureSubImage2D(_handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        
        SetFilter(TextureFilter.Nearest);
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) => GL.TextureSubImage2D(_handle, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);

    public void SetData(ReadOnlySpan<Color> data, int width, int height) => GL.TextureSubImage2D(_handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    
    public void SetFilter(TextureFilter filter) {
        GL.TextureParameteri(_handle, TextureParameterName.TextureMagFilter, filter.MagFilter);
        GL.TextureParameteri(_handle, TextureParameterName.TextureMinFilter, filter.MinFilter);
    }

    public void BindToTextureUnit(uint unit) {
        if (unit > 15) {
            throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
        }
        
        GL.BindTextureUnit(TextureUnit = unit, _handle);
    }
}