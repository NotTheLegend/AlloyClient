using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Common.Rendering;

public sealed class Shader {

    private readonly int _handle;

    private Shader(int handle) {
        _handle = handle;
    }

    public static Shader Create(string path, (string, string)[] defines = null) {
        var p1 = path + ".vert";
        var p2 = path + ".frag";
        var vs = new StringBuilder(File.ReadAllText(p1));
        var fs = new StringBuilder(File.ReadAllText(p2));

        if (defines != null) {
            foreach (var def in defines) {
                vs.Replace(def.Item1, def.Item2);
                fs.Replace(def.Item1, def.Item2);
            }
        }
        
        
        var vertexHandle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexHandle, vs.ToString());
        CompileShader(vertexHandle, p1);
        
        var fragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentHandle, fs.ToString());
        CompileShader(fragmentHandle, p2);
        
        var handle = GL.CreateProgram();
        GL.AttachShader(handle, vertexHandle);
        GL.AttachShader(handle, fragmentHandle);
        LinkProgram(handle);
        
        GL.DetachShader(handle, vertexHandle);
        GL.DetachShader(handle, fragmentHandle);
        GL.DeleteShader(vertexHandle);
        GL.DeleteShader(fragmentHandle);
        
        

        return new Shader(handle);
    }
    
    public void Apply() => GL.UseProgram(_handle);

    public void SetValue(string uniform, Matrix4 matrix) {
        GL.ProgramUniformMatrix4f(_handle, GL.GetUniformLocation(_handle, uniform), 1, true, in matrix);
    }
    
    public void SetValue(string uniform, float value) {
        GL.ProgramUniform1f(_handle, GL.GetUniformLocation(_handle, uniform), value);
    }
    
    public void SetValue(string uniform, Vector2 value) {
        GL.ProgramUniform2f(_handle, GL.GetUniformLocation(_handle, uniform),1, in value);
    }
    
    public void SetValue(string uniform, Vector4[] value) {
        GL.ProgramUniform4f(_handle, GL.GetUniformLocation(_handle, uniform), value.Length, new ReadOnlySpan<Vector4>(value));
    }
    
    public void SetValue(string uniform, Texture texture) {
        GL.ProgramUniform1i(_handle, GL.GetUniformLocation(_handle, uniform), texture.TextureSlot);
    }
    
    private static void CompileShader(int shader, string path)
    {
        GL.CompileShader(shader);

        GL.GetShaderi(shader, ShaderParameterName.CompileStatus, out int code);
        if (code != (int)All.True)
        {
            GL.GetShaderInfoLog(shader, out var infoLog);
            throw new Exception($"Error compiling shader {path}.{Environment.NewLine}{infoLog}");
        }
    }
    
    private static void LinkProgram(int handle) {
        GL.LinkProgram(handle);

        GL.GetProgrami(handle, ProgramProperty.LinkStatus, out int code);

        if (code != (int)All.True) {
            GL.GetProgramInfoLog(handle, out var info);
            throw new Exception($"Error linking shader.{Environment.NewLine}{info}");
        }
    }
}