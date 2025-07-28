using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Common.Rendering;

public sealed class Shader {

    private readonly int _handle;

    private Shader(int handle) {
        _handle = handle;
    }

    public static Shader Create(string path) {
        var vs = File.ReadAllText(path + ".vs");
        var fs = File.ReadAllText(path + ".fs");
        
        var vertexHandle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexHandle, vs);
        CompileShader(vertexHandle);
        
        var fragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentHandle, fs);
        CompileShader(fragmentHandle);
        
        var handle = GL.CreateProgram();
        GL.AttachShader(handle, vertexHandle);
        GL.AttachShader(handle, fragmentHandle);
        GL.LinkProgram(handle);
        
        GL.DetachShader(handle, vertexHandle);
        GL.DetachShader(handle, fragmentHandle);
        GL.DeleteShader(vertexHandle);
        GL.DeleteShader(fragmentHandle);

        return new Shader(handle);
    }
    
    public void Apply() => GL.UseProgram(_handle);

    public void SetValue(string uniform, Matrix4 matrix) {
        GL.UniformMatrix4f(GL.GetUniformLocation(_handle, uniform), 1, true, in matrix);
    }
    
    public void SetValue(string uniform, float value) {
        GL.Uniform1f(GL.GetUniformLocation(_handle, uniform), value);
    }
    
    public void SetValue(string uniform, Vector2 value) {
        GL.Uniform2f(GL.GetUniformLocation(_handle, uniform),1, in value);
    }
    
    public void SetValue(string uniform, Vector4[] value) {
        GL.Uniform4f(GL.GetUniformLocation(_handle, uniform), value.Length, new ReadOnlySpan<Vector4>(value));
    }
    
    public void SetValue(string uniform, Texture texture) {
        GL.Uniform1i(GL.GetUniformLocation(_handle, uniform), texture.TextureSlot);
    }
    
    private static void CompileShader(int shader)
    {
        GL.CompileShader(shader);

        GL.GetShaderi(shader, ShaderParameterName.CompileStatus, out int code);
        if (code != (int)All.True)
        {
            GL.GetShaderInfoLog(shader, out var infoLog);
            throw new Exception($"Error compiling shader.{Environment.NewLine}{infoLog}");
        }
    }
}