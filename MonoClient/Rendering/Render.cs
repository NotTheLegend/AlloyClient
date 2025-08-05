using Common;
using Common.ContentReaders;
using Common.Rendering;
using MonoClient.Assets;
using MonoClient.Rendering.VertexData;
using MonoClient.UiLib;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MonoClient.Rendering;

public static partial class Render {
    
    private const int BufferSize = 1000;
    private const int TileBufferSize = (int) (Map.TileRenderDistance * Map.TileRenderDistance * MathHelper.Pi);
    
    // Shaders
    private static Shader _shaderGround;
    private static Shader _shaderShadow;
    private static Shader _shaderObject;
    private static Shader _shaderParticle;

    private static int _defaultVao;

    // Buffers
    private static IndexBuffer _modelIndexBuffer;
    private static VertexBuffer<VertexBase> _modelVertexBuffer;
    
    private static TileData[] _tileData;
    private static StorageBuffer<TileData> _tileBuffer;
    
    private static ShadowData[] _shadowData;
    private static StorageBuffer<ShadowData> _shadowBuffer;
    
    private static int _entityVao;
    private static VertexObject[] _entityData;
    private static VertexBuffer<VertexObject> _entityDataBuffer;
    

    public static void FirstTimeInit() {
       var alphas = new Vector4[8];
        for (var i = 0; i < 8; i++) {
            alphas[i] = Main.Atlas.GetAtlasData("tileAlphaBlend", i).ToVector4(true);
        }
        
        // Shaders
        _shaderGround = ContentReader.LoadShader("Shaders/Ground");
        _shaderGround.Apply();
        _shaderGround.SetValue("AlphaBlends", alphas);
        _shaderGround.SetValue("GameTexture", Main.Atlas.GetTexture());

        _shaderShadow = ContentReader.LoadShader("Shaders/Shadow");
        
        _shaderObject = ContentReader.LoadShader("Shaders/Object");
        _shaderObject.Apply();
        _shaderObject.SetValue("GameTexture", Main.Atlas.GetTexture());
        
        _shaderObject.SetValue("PixelRange", UiRender.MyriadPro.PixelRange);
        _shaderObject.SetValue("TextTextureSize", new Vector2(UiRender.MyriadPro.Atlas.Width, UiRender.MyriadPro.Atlas.Height));
        _shaderObject.SetValue("TextTexture", UiRender.MyriadPro.Atlas);

        _shaderParticle = ContentReader.LoadShader("Shaders/Particle");
        _shaderParticle.Apply();
        _shaderParticle.SetValue("GameTexture", Main.Atlas.GetTexture());
        
        _defaultVao = GL.GenVertexArray();
        
        _tileData = new TileData[TileBufferSize];
        _tileBuffer = new StorageBuffer<TileData>(TileData.Size, _tileData.Length, BufferUsage.DynamicDraw);

        _shadowData = new ShadowData[BufferSize];
        _shadowBuffer = new StorageBuffer<ShadowData>(ShadowData.Size, _shadowData.Length, BufferUsage.DynamicDraw);
        
        BuildParticleBuffers();
        return;

        // Buffers
        _modelIndexBuffer = new IndexBuffer(ModelData.Indices.Length, BufferUsage.StaticDraw);
        _modelIndexBuffer.Bind();
        _modelIndexBuffer.SetData(ModelData.Indices);

        _modelVertexBuffer = new VertexBuffer<VertexBase>(VertexBase.VertexStride, ModelData.Vertices.Length, BufferUsage.StaticDraw);
        _modelVertexBuffer.Bind();
        _modelVertexBuffer.SetData(ModelData.Vertices);
 
        GL.BindVertexArray(_entityVao = GL.GenVertexArray());
        _entityData = new VertexObject[BufferSize];
        _entityDataBuffer = new VertexBuffer<VertexObject>(VertexObject.VertexStride, BufferSize, BufferUsage.DynamicDraw);
        _entityDataBuffer.Bind();
        _modelIndexBuffer.Bind();
        _modelVertexBuffer.Bind();
    }
    
    public static void SetShaderParams(GameTime gameTime) {
        _shaderGround.Apply();
        _shaderGround.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderGround.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderGround.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderGround.SetValue("GameTime", (float)(gameTime.TotalMs / 1000.0f));
        
        _shaderShadow.Apply();
        _shaderShadow.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderShadow.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderShadow.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderShadow.SetValue("BillMatrix", Camera.BillboardMatrix);
        
        _shaderObject.Apply();
        _shaderObject.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderObject.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderObject.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderObject.SetValue("BillMatrix", Camera.BillboardMatrix);
        
        _shaderParticle.Apply();
        _shaderParticle.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderParticle.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderParticle.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderParticle.SetValue("BillMatrix", Camera.BillboardMatrix);
    }
}