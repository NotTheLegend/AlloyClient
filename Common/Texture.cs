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
    
    private Texture(int handle, int slot, int width, int height) {
        Handle = handle;
        TextureSlot = slot;
        Width = width;
        Height = height;
        //TODO: add param for filters, and stuff
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) {
        GL.BindTexture(TextureTarget.Texture2d, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2d, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void SetData(ReadOnlySpan<Color> data, int width, int height) {
        GL.BindTexture(TextureTarget.Texture2d, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public static Texture FromStream(Stream stream, TextureFilter filter) {
        var (handle, slot) = CreateHandle();
        
        using var img = StbImage.Load(stream, StbiImageFormat.Rgba);
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.ImagePointer);
        SetFilters(filter);
        GL.GenerateMipmap(TextureTarget.Texture2d);
        
        return new Texture(handle, slot, img.Width, img.Height);
    }

    public static Texture FromFile(string file, TextureFilter filter) {
        using var stream = File.Open(file, FileMode.Open);
        return FromStream(stream, filter);
    }

    public static Texture Create(ReadOnlySpan<Color> data, int width, int height, TextureFilter filter) {
        var (handle, slot) = CreateHandle();
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        SetFilters(filter);
        GL.GenerateMipmap(TextureTarget.Texture2d);
        return new Texture(handle, slot, width, height);
    }

    private static (int, int) CreateHandle() {
        var handle = GL.GenTexture();
        var slot = _textureCount;
        
        GL.ActiveTexture(IntToTexUnit(slot));
        GL.BindTexture(TextureTarget.Texture2d, handle);
        
        _textureCount++;
        
        return (handle, slot);
    }

    private static void SetFilters(TextureFilter filter) {
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, filter.MagFilter);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, filter.MinFilter);
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

public readonly struct TextureFilter {
    
    public static readonly TextureFilter Nearest = new (All.Nearest, All.Nearest);
    
    public static readonly TextureFilter Linear = new (All.Linear, All.Linear);

    public readonly int MagFilter;

    public readonly int MinFilter;

    private TextureFilter(All mag, All min) {
        Check(mag);
        Check(min);
        
        MagFilter = (int)mag;
        MinFilter = (int)min;
    }
    
    private static void Check(All val) {
        switch (val) {
            case All.Nearest:
            case All.Linear:
                break;
            default:
                throw new Exception("Not a valid texture filter");
        }
    }
}