namespace AlloyClient.Engine.Graphics;

public sealed class Shader {

    internal record struct UniformInfo(int Location, UniformType Type, int Size);

    public readonly string Name;

    private readonly int _handle;

    private readonly Dictionary<string, UniformInfo> _uniforms = new();

    public Shader(string path, (string, int)[] defines = null) {
        Name = new DirectoryInfo(path).Name;;
        _handle = GL.CreateProgram();
        
        ShaderHelper.Compile(_handle, path, defines);
        ShaderHelper.LoadUniforms(_handle, _uniforms);
    }

    public void Apply() => GL.UseProgram(_handle);

    public void SetValue(string uniform, Matrix4 matrix) => GL.ProgramUniformMatrix4f(_handle, GetLocation(uniform, UniformType.FloatMat4), 1, true, in matrix);

    public void SetValue(string uniform, float value) => GL.ProgramUniform1f(_handle, GetLocation(uniform, UniformType.Float), value);

    public void SetValue(string uniform, Vector2 value) => GL.ProgramUniform2f(_handle, GetLocation(uniform, UniformType.FloatVec2), 1, in value);

    public void SetValue(string uniform, Texture texture) => GL.ProgramUniform1i(_handle, GetLocation(uniform, UniformType.Sampler2d), (int)texture.TextureUnit);

    private int GetLocation(string uniform, UniformType type) {
        if (!_uniforms.TryGetValue(uniform, out var info)) {
            throw new Exception($"Unable to find uniform <{uniform}> in shader <{Name}>");
        }

        if (type != info.Type) {
            throw new Exception($"Value does not match the type of uniform <{uniform}>");
        }

        return info.Location;
    }
}