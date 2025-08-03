using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using ReFuel.Stb;

namespace Common;

public sealed class Texture {

    private static int _textureCount;

    public readonly int Handle;

    public readonly int TextureSlot;
    
    public readonly int Width;
    
    public readonly int Height;
    
    public Texture(int handle, int width, int height) {
        Handle = handle;
        Width = width;
        Height = height;
        //TODO: add param for filters, and stuff
        GL.ActiveTexture(IntToTexUnit(_textureCount));
        GL.BindTexture(TextureTarget.Texture2d, Handle);
        TextureSlot = _textureCount;
        _textureCount++;
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) {
        GL.BindTexture(TextureTarget.Texture2d, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2d, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void SetData(ReadOnlySpan<Color> data, int width, int height) {
        GL.BindTexture(TextureTarget.Texture2d, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public static Texture FromStream(Stream stream) {
        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, handle);
        
        using var img = StbImage.Load(stream, StbiImageFormat.Rgba);
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.ImagePointer);
        GL.GenerateMipmap(TextureTarget.Texture2d);
        
        return new Texture(handle, img.Width, img.Height);
    }

    public static Texture FromFile(string file) {
        using var stream = File.Open(file, FileMode.Open);
        return FromStream(stream);
    }

    public static Texture Create(ReadOnlySpan<Color> data, int width, int height) {
        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, handle);
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        GL.GenerateMipmap(TextureTarget.Texture2d);
        return new Texture(handle, width, height);
    }

    private static TextureUnit IntToTexUnit(int value) => value switch {
        0 => TextureUnit.Texture0,
        1 => TextureUnit.Texture1,
        2 => TextureUnit.Texture2,
        3 => TextureUnit.Texture3,
        4 => TextureUnit.Texture4,
        5 => TextureUnit.Texture5,
        6 => TextureUnit.Texture6,
        7 => TextureUnit.Texture7,
        8 => TextureUnit.Texture8,
        9 => TextureUnit.Texture9,
        10 => TextureUnit.Texture10,
        11 => TextureUnit.Texture11,
        12 => TextureUnit.Texture12,
        13 => TextureUnit.Texture13,
        14 => TextureUnit.Texture14,
        15 => TextureUnit.Texture15,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
    
    private static int TexUnitToInt(TextureUnit value) => value switch {
        TextureUnit.Texture0 => 0,
        TextureUnit.Texture1 => 1,
        TextureUnit.Texture2 => 2,
        TextureUnit.Texture3 => 3,
        TextureUnit.Texture4 => 4,
        TextureUnit.Texture5 => 5,
        TextureUnit.Texture6 => 6,
        TextureUnit.Texture7 => 7,
        TextureUnit.Texture8 => 8,
        TextureUnit.Texture9 => 9,
        TextureUnit.Texture10 => 10,
        TextureUnit.Texture11 => 11,
        TextureUnit.Texture12 => 12,
        TextureUnit.Texture13 => 13,
        TextureUnit.Texture14 => 14,
        TextureUnit.Texture15 => 15,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

}