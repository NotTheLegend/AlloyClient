using Alloy.Common.SourceGen;
using Alloy.Engine.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Editor;

public sealed partial class EditorBackdropRenderer {
    [Shader("EditorBackdrop")] private static partial ShaderSource BackdropShaderSource { get; }

    private readonly Shader _shader;
    private readonly Sampler _sampler;
    private readonly VertexArrayObject _vao;
    private readonly float _textureAspect;

    public EditorBackdropRenderer() {
        var atlas = Main.UiAtlas.GetAtlasData("MapEditor/Background", 0);
        atlas.RemovePadding();
        _textureAspect = atlas.RawW() / (float)atlas.RawH();
        _sampler = new Sampler(Main.UiAtlas.Texture, 7);
        _shader = Shader.FromSource(BackdropShaderSource);
        _shader.SetValue("BackdropTexture", _sampler);
        _shader.SetValue("UvOrigin", new Vector2(atlas.U, atlas.V));
        _shader.SetValue("UvSize", new Vector2(atlas.W, atlas.H));
        _shader.SetValue("UvScale", Vector2.One);
        _vao = new VertexArrayObject();
    }

    public void Resize(int width, int height) {
        if (width <= 0 || height <= 0) return;
        var screenAspect = width / (float)height;
        var uvScale = screenAspect > _textureAspect
            ? new Vector2(1f, _textureAspect / screenAspect)
            : new Vector2(screenAspect / _textureAspect, 1f);
        _shader.SetValue("UvScale", uvScale);
    }

    public void Draw() {
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        _shader.Apply();
        _vao.Bind();
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}
