using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.Assets;
using MonoClient.Rendering.VertexData;
using MonoClient.UiLib;

namespace MonoClient.Rendering;

public static partial class Render {
    
    private const int BufferSize = 1000;
    private const int TileBufferSize = (int) (Map.TileRenderDistance * Map.TileRenderDistance * MathHelper.Pi);
    
    private static GraphicsDevice _graphics;
    
    // Shaders
    private static Effect _shaderGround;
    private static Effect _shaderShadow;
    private static Effect _shaderObject;
    private static Effect _shaderParticle;

    // Buffers
    private static IndexBuffer _modelIndexBuffer;
    private static VertexBuffer _modelVertexBuffer;
    private static VertexBufferBinding _modelVertexBinding;

    private static VertexTile[] _tileData;
    private static DynamicVertexBuffer _tileDataBuffer;
    private static VertexBufferBinding _tileDataBinding;
    
    private static VertexTile[] _editorTileData;
    private static DynamicVertexBuffer _editorTileDataBuffer;
    private static VertexBufferBinding _editorTileDataBinding;
    
    private static VertexShadow[] _shadowData;
    private static DynamicVertexBuffer _shadowDataBuffer;
    private static VertexBufferBinding _shadowDataBinding;
    
    private static VertexObject[] _entityData;
    private static DynamicVertexBuffer _entityDataBuffer;
    private static VertexBufferBinding _entityDataBinding;

    public static void FirstTimeInit() {
        _graphics = Main.Graphics.GraphicsDevice;

        var alphas = new Vector4[8];
        for (var i = 0; i < 8; i++) {
            alphas[i] = Main.Atlas.AtlasMapStatic["tileAlphaBlend"][i].ToVector4(true);
        }
        
        // Shaders
        _shaderGround = Main.ContentManager.Load<Effect>("Shaders/ShaderGround");
        _shaderGround.Parameters["GameTexture"].SetValue(Main.Atlas.Texture);
        _shaderGround.Parameters["AlphaBlends"].SetValue(alphas);

        _shaderShadow = Main.ContentManager.Load<Effect>("Shaders/ShaderShadow");
        
        _shaderObject = Main.ContentManager.Load<Effect>("Shaders/ShaderObject");
        _shaderObject.Parameters["GameTexture"].SetValue(Main.Atlas.Texture);
        
        _shaderObject.Parameters["PixelRange"].SetValue(UiRender.MyriadPro.PixelRange);
        _shaderObject.Parameters["TextTextureSize"].SetValue(new Vector2(UiRender.MyriadPro.Atlas.Width, UiRender.MyriadPro.Atlas.Height));
        _shaderObject.Parameters["TextTexture"].SetValue(UiRender.MyriadPro.Atlas);

        _shaderParticle = Main.ContentManager.Load<Effect>("Shaders/ShaderParticle");
        _shaderParticle.Parameters["GameTexture"].SetValue(Main.Atlas.Texture);
        
        // Buffers
        _modelIndexBuffer = new IndexBuffer(_graphics, IndexElementSize.SixteenBits, ModelData.Indices.Length, BufferUsage.WriteOnly);
        _modelIndexBuffer.SetData(ModelData.Indices);

        _modelVertexBuffer = new VertexBuffer(_graphics, VertexBase.VertexDeclaration, ModelData.Vertices.Length, BufferUsage.WriteOnly);
        _modelVertexBuffer.SetData(ModelData.Vertices);
        _modelVertexBinding = new VertexBufferBinding(_modelVertexBuffer);
        
        _tileData = new VertexTile[TileBufferSize];
        _tileDataBuffer = new DynamicVertexBuffer(_graphics, VertexTile.VertexDeclaration, _tileData.Length, BufferUsage.WriteOnly);
        _tileDataBinding = new VertexBufferBinding(_tileDataBuffer, 0, 1);
        
        _editorTileData = new VertexTile[BufferSize];
        _editorTileDataBuffer = new DynamicVertexBuffer(_graphics, VertexTile.VertexDeclaration, _editorTileData.Length, BufferUsage.WriteOnly);
        _editorTileDataBinding = new VertexBufferBinding(_editorTileDataBuffer, 0, 1);

        _shadowData = new VertexShadow[BufferSize];
        _shadowDataBuffer = new DynamicVertexBuffer(_graphics, VertexShadow.VertexDeclaration, BufferSize, BufferUsage.WriteOnly);
        _shadowDataBinding = new VertexBufferBinding(_shadowDataBuffer, 0, 1);
        
        _entityData = new VertexObject[BufferSize];
        _entityDataBuffer = new DynamicVertexBuffer(_graphics, VertexObject.VertexDeclaration, BufferSize, BufferUsage.WriteOnly);
        _entityDataBinding = new VertexBufferBinding(_entityDataBuffer, 0, 1);
        
        BuildParticleBuffers();
    }
    
    public static void SetShaderParams(GameTime gameTime) {
        _shaderGround.Parameters["WorldMatrix"].SetValue(Camera.WorldMatrix);
        _shaderGround.Parameters["ViewMatrix"].SetValue(Camera.ViewMatrix);
        _shaderGround.Parameters["ProjMatrix"].SetValue(Camera.ProjectionMatrix);
        _shaderGround.Parameters["GameTime"].SetValue((float)(gameTime.TotalGameTime.TotalMilliseconds / 1000.0f));
        
        _shaderShadow.Parameters["WorldMatrix"].SetValue(Camera.WorldMatrix);
        _shaderShadow.Parameters["ViewMatrix"].SetValue(Camera.ViewMatrix);
        _shaderShadow.Parameters["ProjMatrix"].SetValue(Camera.ProjectionMatrix);
        
        _shaderObject.Parameters["WorldMatrix"].SetValue(Camera.WorldMatrix);
        _shaderObject.Parameters["ViewMatrix"].SetValue(Camera.ViewMatrix);
        _shaderObject.Parameters["ProjMatrix"].SetValue(Camera.ProjectionMatrix);
        _shaderObject.Parameters["BillMatrix"].SetValue(Camera.BillboardMatrix);
        
        _shaderParticle.Parameters["WorldMatrix"].SetValue(Camera.WorldMatrix);
        _shaderParticle.Parameters["ViewMatrix"].SetValue(Camera.ViewMatrix);
        _shaderParticle.Parameters["ProjMatrix"].SetValue(Camera.ProjectionMatrix);
        _shaderParticle.Parameters["BillMatrix"].SetValue(Camera.BillboardMatrix);
    }
}