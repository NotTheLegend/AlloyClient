using System;
using AlloyClient.Assets;
using AlloyClient.Engine.Graphics;
using AlloyClient.Engine.Graphics.Buffers;
using AlloyClient.Game;
using AlloyClient.Rendering.VertexData;
using AlloyClient.UiLib;
using Common;
using Common.ContentReaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering;

public static partial class Render {
    
    private const int BufferSize = 1000;
    private const int TileBufferSize = (int) (Map.TileRenderDistance * Map.TileRenderDistance * MathHelper.Pi) * 3;
    private const int ShadowBufferSize = 4096;
    
    private static readonly (string, string)[] TileDefines = [("TileBuffer", $"{TileBufferSize}")];
    private static readonly (string, string)[] ShadowDefines = [("ShadowBuffer", $"{ShadowBufferSize}")];
    
    // Shaders
    private static Shader _shaderGround;
    private static Shader _shaderShadow;
    private static Shader _shaderModel;
    private static Shader _shaderObject;
    private static Shader _shaderParticle;
    
    // Vertex Objects
    private static VertexArrayObject _defaultVao;
    private static VertexArrayObject _modelVao;

    // Buffers
    private static IndexBuffer _modelIndexBuffer;
    private static VertexBuffer<VertexBase> _modelVertexBuffer;
    
    private static TileData[] _tileData;
    private static StorageBuffer<TileData> _tileBuffer;
    
    private static ShadowData[] _shadowData;
    private static UniformBuffer _shadowBuffer;
    
    private static VertexModel[] _modelData;
    private static VertexBuffer<VertexModel> _modelDataBuffer;
    
    //private static int _entityVao;
    //private static VertexObject[] _entityData;
    //private static VertexBuffer<VertexObject> _entityDataBuffer;
    

    public static unsafe void FirstTimeInit() {
        // Shaders
        _shaderGround = ContentReader.LoadShader("Shaders/Ground", TileDefines);
        _shaderGround.SetValue("GameTexture", Main.Atlas.Texture);

        _shaderShadow = ContentReader.LoadShader("Shaders/Shadow", ShadowDefines);
        
        _shaderModel = ContentReader.LoadShader("Shaders/Model");
        _shaderModel.SetValue("GameTexture", Main.Atlas.Texture);
        
        _shaderObject = ContentReader.LoadShader("Shaders/Object");
        _shaderObject.SetValue("GameTexture", Main.Atlas.Texture);
        
        _shaderObject.SetValue("PixelRange", UiRender.MyriadPro.PixelRange);
        _shaderObject.SetValue("TextTextureSize", new Vector2(UiRender.MyriadPro.Atlas.Width, UiRender.MyriadPro.Atlas.Height));
        _shaderObject.SetValue("TextTexture", UiRender.MyriadPro.Atlas);

        _shaderParticle = ContentReader.LoadShader("Shaders/Particle");
        _shaderParticle.SetValue("GameTexture", Main.Atlas.Texture);
        
        _defaultVao = new VertexArrayObject();
        
        _tileData = new TileData[TileBufferSize];
        _tileBuffer = new StorageBuffer<TileData>(_tileData.Length);

        _shadowData = new ShadowData[ShadowBufferSize];
        _shadowBuffer = new UniformBuffer(_shadowData.Length * sizeof(ShadowData));
        
        _modelIndexBuffer = new IndexBuffer(ModelData.Indices.Length);
        _modelIndexBuffer.SetData(ModelData.Indices);
        _modelVertexBuffer = new VertexBuffer<VertexBase>(VertexBase.VertexStride, ModelData.Vertices.Length);
        _modelVertexBuffer.SetData(ModelData.Vertices);

        _modelVao = new VertexArrayObject();
        
        _modelData = new VertexModel[BufferSize];
        _modelDataBuffer = new VertexBuffer<VertexModel>(VertexModel.VertexStride, BufferSize);
        _modelIndexBuffer.BindTo(_modelVao);
        _modelVertexBuffer.BindTo(_modelVao);
        _modelDataBuffer.BindTo(_modelVao, 1);
        
        /*
        GL.BindVertexArray(_entityVao = GL.GenVertexArray());
        _entityData = new VertexObject[BufferSize];
        _entityDataBuffer = new VertexBuffer<VertexObject>(VertexObject.VertexStride, BufferSize, BufferUsage.DynamicDraw);
        _entityDataBuffer.Bind();
        _modelIndexBuffer.Bind();
        _modelVertexBuffer.Bind();
        
        */
        BuildParticleBuffers();
    }
    
    public static void SetShaderParams(GameTime gameTime) {
        _shaderGround.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderGround.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderGround.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderGround.SetValue("GameTime", (float)(gameTime.TotalMs / 1000.0f));
        
        _shaderShadow.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderShadow.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderShadow.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderShadow.SetValue("BillMatrix", Camera.BillboardMatrix);
        
        _shaderModel.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderModel.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderModel.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        
        _shaderObject.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderObject.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderObject.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderObject.SetValue("BillMatrix", Camera.BillboardMatrix);
        
        _shaderParticle.SetValue("WorldMatrix", Camera.WorldMatrix);
        _shaderParticle.SetValue("ViewMatrix", Camera.ViewMatrix);
        _shaderParticle.SetValue("ProjMatrix", Camera.ProjectionMatrix);
        _shaderParticle.SetValue("BillMatrix", Camera.BillboardMatrix);
    }
}