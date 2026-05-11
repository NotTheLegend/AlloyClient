using Alloy.Engine.Graphics.Buffers;

namespace Alloy.Engine.Graphics;

public sealed class Shader {

    internal record struct UniformInfo(int Location, UniformType Type, int Size);
    
    internal record struct UniformBlockInfo(uint Location);

    public readonly string Name;

    internal readonly int Handle;

    private readonly Dictionary<string, UniformInfo> _uniforms = new();
    
    private readonly Dictionary<string, UniformBlockInfo> _uniformBlocks = new();

    public Shader(string path, (string, string)[] defines = null) {
        Name = new DirectoryInfo(path).Name;
        Handle = GL.CreateProgram();
        
        ShaderHelper.Compile(Handle, path, defines);
        ShaderHelper.LoadUniformProperties(Handle, _uniforms);
        ShaderHelper.LoadUniformBlocks(Handle, _uniformBlocks);
    }

    public void Apply() => GL.UseProgram(Handle);

    public void SetValue(string uniform, Matrix4 matrix) => GL.ProgramUniformMatrix4f(Handle, GetLocation(uniform, UniformType.FloatMat4), 1, true, in matrix);

    public void SetValue(string uniform, float value) => GL.ProgramUniform1f(Handle, GetLocation(uniform, UniformType.Float), value);
    
    public void SetValue(string uniform, int value) => GL.ProgramUniform1i(Handle, GetLocation(uniform, UniformType.Int), value);

    public void SetValue(string uniform, Vector2 value) => GL.ProgramUniform2f(Handle, GetLocation(uniform, UniformType.FloatVec2), 1, in value);

    //public void SetValue(string uniform, Texture texture) => GL.ProgramUniform1i(Handle, GetLocation(uniform, UniformType.Sampler2d), (int)texture.TextureUnit);
    
    public void SetValue(string uniform, UniformBuffer buffer) => GL.BindBufferBase(BufferTarget.UniformBuffer, GetUniformBlock(uniform), buffer.Handle);
    
    public void SetValue(string uniform, Sampler sampler) => GL.ProgramUniform1i(Handle, GetLocation(uniform, UniformType.Sampler2d), (int)sampler.TextureUnit);

    private int GetLocation(string uniform, UniformType type) {
        if (!_uniforms.TryGetValue(uniform, out var info)) {
            throw new Exception($"Unable to find uniform <{uniform}> in shader <{Name}>");
        }

        if (type != info.Type) {
            throw new Exception($"Value does not match the type of uniform <{uniform}>");
        }

        return info.Location;
    }

    internal uint GetUniformBlock(string uniform) {
        if (!_uniformBlocks.TryGetValue(uniform, out var info)) {
            throw new Exception($"Unable to find uniform block <{uniform}> in shader <{Name}>");
        }

        return info.Location;
    }
}