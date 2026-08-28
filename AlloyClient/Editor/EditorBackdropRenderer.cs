using Alloy.Common.SourceGen;
using Alloy.Engine.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Editor;

public sealed partial class EditorBackdropRenderer {
    [Shader("EditorBackdrop")] private static partial ShaderSource BackdropShaderSource { get; }

    private static Shader _shader;
    private static VertexArrayObject _vao;
    private static Sampler _sampler;
    private static float _textureAspect;
    private static bool _initialized;

    public EditorBackdropRenderer() {
        if (_initialized) {
            return;
        }

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
        _initialized = true;
    }

    public void Resize(int width, int height) {
        if (width <= 0 || height <= 0) {
            return;
        }

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

    public static void Dispose() {
        if (!_initialized) {
            return;
        }

        _vao.Dispose();
        _shader.Dispose();
        _sampler.Dispose();
        _initialized = false;
    }
}
