namespace Alloy.Engine.Graphics;

public sealed class Sampler : IDisposable {

    internal int Handle;

    internal readonly int TextureHandle;

    internal uint TextureUnit;
    
    public Sampler(Texture texture) {
        GL.CreateSampler(out Handle);
        TextureHandle = texture.Handle;
        SetFilter(TextureFilter.Nearest);
    }
    
    public Sampler(Texture texture, uint textureUnit) {
        GL.CreateSampler(out Handle);
        TextureHandle = texture.Handle;
        SetFilter(TextureFilter.Nearest);
        Bind(textureUnit);
    }

    public Sampler(Texture texture, TextureFilter filter) {
        GL.CreateSampler(out Handle);
        TextureHandle = texture.Handle;
        SetFilter(filter);
    }
    
    public Sampler(Texture texture, TextureFilter filter, uint textureUnit) {
        GL.CreateSampler(out Handle);
        TextureHandle = texture.Handle;
        Bind(textureUnit);
        SetFilter(filter);
    }

    public void Bind(uint textureUnit) {
        if (textureUnit > 15) {
            throw new ArgumentOutOfRangeException(nameof(textureUnit), textureUnit, null);
        }
        
        
        TextureUnit = textureUnit;
        GL.BindTextureUnit(textureUnit, TextureHandle);
        GL.BindSampler(textureUnit, Handle);
    }
    
    public void SetFilter(TextureFilter filter) {
        GL.SamplerParameterIi(Handle, SamplerParameterI.TextureMagFilter, in filter.MagFilter);
        GL.SamplerParameterIi(Handle, SamplerParameterI.TextureMinFilter, in filter.MinFilter);
    }

    public void Delete() {
        Dispose();
    }

    public void Dispose() {
        if (Handle == 0) {
            return;
        }

        GL.DeleteSampler(Handle);
        Handle = 0;
    }

}