using System.Text;

namespace AlloyClient.Engine.Graphics;

internal static class ShaderHelper {

    internal static void LoadUniformProperties(int handle, Dictionary<string, Shader.UniformInfo> uniforms) {
        GL.GetProgrami(handle, ProgramProperty.ActiveUniforms, out var count);
        if (count == 0)
            return;

        GL.GetProgrami(handle, ProgramProperty.ActiveUniformMaxLength, out var maxLength);
        for (var i = 0u; i < count; i++) {
            var uniform = GL.GetActiveUniform(handle, i, maxLength, out _, out var size, out var type);
            var location = GL.GetUniformLocation(handle, uniform);
            uniforms[uniform] = new Shader.UniformInfo(location, type, size);
        }
    }
    
    internal static void LoadUniformBlocks(int handle, Dictionary<string, Shader.UniformBlockInfo> blocks) {
        GL.GetProgramInterfacei(handle, ProgramInterface.UniformBlock, ProgramInterfacePName.ActiveResources, out var count);
        if (count == 0)
            return;
        
        GL.GetProgramInterfacei(handle, ProgramInterface.UniformBlock, ProgramInterfacePName.MaxNameLength, out var maxLength);
        for (var i = 0u; i < count; i++) {

            var blockName = GL.GetProgramResourceName(handle, ProgramInterface.UniformBlock, i, maxLength, out _);
            blocks[blockName] = new Shader.UniformBlockInfo(i);
        }
    }

    internal static void Compile(int handle, string path, (string, string)[] defines) {
        var p1 = path + ".vert";
        var p2 = path + ".frag";
        var vs = new StringBuilder(File.ReadAllText(p1));
        var fs = new StringBuilder(File.ReadAllText(p2));

        if (defines != null) {
            foreach (var def in defines) {
                var txt1 = $"#define {def.Item1}";
                var txt2 = $"{txt1} {def.Item2}";
                vs.Replace(txt1, txt2);
                fs.Replace(txt1, txt2);
            }
        }

        var vertexHandle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexHandle, vs.ToString());
        CompileShader(vertexHandle, p1);

        var fragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentHandle, fs.ToString());
        CompileShader(fragmentHandle, p2);
        
        GL.AttachShader(handle, vertexHandle);
        GL.AttachShader(handle, fragmentHandle);
        LinkProgram(handle);

        GL.DetachShader(handle, vertexHandle);
        GL.DetachShader(handle, fragmentHandle);
        GL.DeleteShader(vertexHandle);
        GL.DeleteShader(fragmentHandle);
    }
    
    private static void CompileShader(int shader, string path) {
        GL.CompileShader(shader);

        GL.GetShaderi(shader, ShaderParameterName.CompileStatus, out var code);
        if (code != (int)All.True) {
            GL.GetShaderInfoLog(shader, out var infoLog);
            throw new Exception($"Error compiling shader {path}.{Environment.NewLine}{infoLog}");
        }
    }

    private static void LinkProgram(int handle) {
        GL.LinkProgram(handle);

        GL.GetProgrami(handle, ProgramProperty.LinkStatus, out var code);

        if (code != (int)All.True) {
            GL.GetProgramInfoLog(handle, out var info);
            throw new Exception($"Error linking shader.{Environment.NewLine}{info}");
        }
    }
}