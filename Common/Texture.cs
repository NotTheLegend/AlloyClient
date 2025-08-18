using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using ReFuel.Stb;

namespace Common;

public sealed class Texture {

    private static uint _textureCount;

    public readonly int Handle;

    public readonly int TextureSlot;
    
    public readonly int Width;
    
    public readonly int Height;
    
    private Texture(int handle, uint slot, int width, int height, TextureFilter filter) {
        Handle = handle;
        TextureSlot = (int)slot;
        Width = width;
        Height = height;
        SetFilter(filter);
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) {
        GL.TextureSubImage2D(Handle, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void SetData(ReadOnlySpan<Color> data, int width, int height) {
        GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public static Texture FromStream(Stream stream, TextureFilter filter) {
        var (handle, slot) = CreateHandle();
        
        using var img = StbImage.Load(stream, StbiImageFormat.Rgba);
        
        GL.TextureStorage2D(handle, 1, SizedInternalFormat.Rgba8, img.Width, img.Height);
        GL.TextureSubImage2D(handle, 0, 0, 0, img.Width, img.Height, PixelFormat.Rgba, PixelType.UnsignedByte, img.ImagePointer);
        
        return new Texture(handle, slot, img.Width, img.Height, filter);
    }

    public static Texture FromFile(string file, TextureFilter filter) {
        using var stream = File.Open(file, FileMode.Open);
        return FromStream(stream, filter);
    }

    public static Texture Create(ReadOnlySpan<Color> data, int width, int height, TextureFilter filter) {
        var (handle, slot) = CreateHandle();
        
        GL.TextureStorage2D(handle, 1, SizedInternalFormat.Rgba8, width, height);
        GL.TextureSubImage2D(handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        
        return new Texture(handle, slot, width, height, filter);
    }
    
    public void SetFilter(TextureFilter filter) {
        GL.TextureParameteri(Handle, TextureParameterName.TextureMagFilter, filter.MagFilter);
        GL.TextureParameteri(Handle, TextureParameterName.TextureMinFilter, filter.MinFilter);
    }

    private static (int, uint) CreateHandle() {
        var handle = GL.CreateTexture(TextureTarget.Texture2d);
        var slot = _textureCount;
        
        GL.BindTextureUnit(slot, handle);
        
        _textureCount++;
        
        return (handle, slot);
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
}

public readonly struct TextureFilter {
    
    public static readonly TextureFilter Nearest = new (TextureMinFilter.Nearest, TextureMagFilter.Nearest);
    
    public static readonly TextureFilter Linear = new (TextureMinFilter.Linear, TextureMagFilter.Linear);
    
    public readonly int MinFilter;

    public readonly int MagFilter;

    private TextureFilter(TextureMinFilter min, TextureMagFilter mag) {
        Check(min);
        Check(mag);
        
        MinFilter = (int)min;
        MagFilter = (int)mag;
    }
    
    private static void Check(TextureMinFilter val) {
        switch (val) {
            case TextureMinFilter.Nearest:
            case TextureMinFilter.Linear:
                break;
            default:
                throw new Exception("Not a valid texture filter");
        }
    }
    
    private static void Check(TextureMagFilter val) {
        switch (val) {
            case TextureMagFilter.Nearest:
            case TextureMagFilter.Linear:
                break;
            default:
                throw new Exception("Not a valid texture filter");
        }
    }
}