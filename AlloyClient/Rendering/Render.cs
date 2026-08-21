using Alloy.Common.SourceGen;
using Alloy.Engine.Graphics;
using Alloy.Engine.Graphics.Buffers;
using AlloyClient.Assets;
using AlloyClient.Game;
using AlloyClient.Rendering.VertexData;
using Alloy.Engine;
using Alloy.UiLib.Data;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering;

public static partial class Render {
    
    private const int BufferSize = 10000;
    public const int TileBufferSize = Map.VisibleChunks * TileMap.ChunkArea * 4;
    private const int ShadowBufferSize = 4096;
    
    private static readonly (string, string)[] TileDefines = [("TileBuffer", $"{TileBufferSize}")];
    private static readonly (string, string)[] ShadowDefines = [("ShadowBuffer", $"{ShadowBufferSize}")];
    private static readonly (string, string)[] ObjectDefines = [("ObjectBuffer", $"{BufferSize}")];
    
    // Shader Sources
    [Shader("Ground")] private static partial ShaderSource GroundShaderSource { get; }
    [Shader("Shadow")] private static partial ShaderSource ShadowShaderSource { get; }
    [Shader("Model")] private static partial ShaderSource ModelShaderSource { get; }
    [Shader("Object")] private static partial ShaderSource ObjectShaderSource { get; }
    [Shader("Particle")] private static partial ShaderSource ParticleShaderSource { get; }
    
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
    
    private static VertexObject[] _entityData;
    private static StorageBuffer<VertexObject> _entityDataBuffer;
    
    public static unsafe void FirstTimeInit(Sampler atlas, BitmapFamily font) {
        // Shaders
        _shaderGround = Shader.FromSource(GroundShaderSource, TileDefines);
        _shaderGround.SetValue("GameTexture", atlas);

        _shaderShadow = Shader.FromSource(ShadowShaderSource, ShadowDefines);
        
        _shaderModel = Shader.FromSource(ModelShaderSource);
        _shaderModel.SetValue("GameTexture", atlas);
        
        _shaderObject = Shader.FromSource(ObjectShaderSource, ObjectDefines);
        _shaderObject.SetValue("GameTexture", atlas);
        
        _shaderObject.SetValue("PixelRange", font.PixelRange);
        _shaderObject.SetValue("TextTextureSize", new Vector2(font.Atlas.Width, font.Atlas.Height));
        _shaderObject.SetValue("TextTexture", font.Sampler);

        _shaderParticle = Shader.FromSource(ParticleShaderSource);
        
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
        
        
        _entityData = new VertexObject[BufferSize];
        _entityDataBuffer = new StorageBuffer<VertexObject>(BufferSize);
        
        BuildParticleBuffers();
    }
    
    public static void SetShaderParams(GameTime gameTime, Camera camera) {
        _shaderGround.SetValue("FullMatrix", camera.Matrix);
        _shaderGround.SetValue("GameTime", (float)(gameTime.TotalMs / 1000.0f));
        
        _shaderShadow.SetValue("FullMatrix", camera.Matrix);
        _shaderShadow.SetValue("BillMatrix", camera.BillboardMatrix);
        
        _shaderModel.SetValue("FullMatrix", camera.Matrix);
        
        _shaderObject.SetValue("FullMatrix", camera.Matrix);
        _shaderObject.SetValue("BillMatrix", camera.BillboardMatrix);
        _shaderObject.SetValue("Zoom", Settings.CameraZoom);
        
        _shaderParticle.SetValue("FullMatrix", camera.Matrix);
        _shaderParticle.SetValue("BillMatrix", camera.BillboardMatrix);
    }
}