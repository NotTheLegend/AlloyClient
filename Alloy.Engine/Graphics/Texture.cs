using Alloy.Common;
using ReFuel.Stb;
using Alloy.Engine.Diagnostics;

namespace Alloy.Engine.Graphics;

public sealed class Texture : IDisposable {
    
    public readonly int Width;
    
    public readonly int Height;
    
    internal int Handle;

    public Texture(string file) : this(File.ReadAllBytes(file)) { }

    public Texture(ReadOnlySpan<byte> data) {
        using var image = StbImage.Load(data, StbiImageFormat.Rgba);

        Width = image.Width;
        Height = image.Height;
        Handle = Create(image.AsSpan<Color>(), Width, Height);
    }
    
    public Texture(StbImage image) : this(image.AsSpan<Color>(), image.Width, image.Height) { }
    
    public Texture(ReadOnlySpan<Color> data, int width, int height) {
        Width = width;
        Height = height;
        Handle = Create(data, width, height);
    }

    public void SetData(ReadOnlySpan<Color> data, Vector4i rect) {
        FrameMetrics.RecordUpload(data.Length * 4L);
        GL.TextureSubImage2D(Handle, 0, rect.X, rect.Y, rect.Z, rect.W, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void SetData(ReadOnlySpan<Color> data, int width, int height) {
        FrameMetrics.RecordUpload(data.Length * 4L);
        GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void Dispose() {
        if (Handle == 0) {
            return;
        }

        GL.DeleteTexture(Handle);
        Handle = 0;
    }

    private static int Create(ReadOnlySpan<Color> data, int width, int height) {
        var handle = GL.CreateTexture(TextureTarget.Texture2D);

        GL.TextureStorage2D(handle, 1, SizedInternalFormat.Rgba8, width, height);
        GL.TextureSubImage2D(handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        return handle;
    }
}
