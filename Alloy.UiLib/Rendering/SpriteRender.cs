using System;
using Alloy.Engine.Graphics;
using Alloy.Engine.Graphics.Buffers;
using OpenTK.Graphics.OpenGL;

namespace Alloy.UiLib.Rendering;

public static class SpriteRender {

    private const int InstanceBufferSize = 1000;
    private const int VertexBufferSize = InstanceBufferSize * 6; // Most sprites are a quad which has 6 vertices

    private static ushort _instanceCount;
    private static SpriteInstanceData[] _instanceData; 
    private static StorageBuffer<SpriteInstanceData> _instanceBuffer;
    
    private static int _vertexCount;
    private static SpriteVertexData[] _vertices;
    private static VertexBuffer<SpriteVertexData> _vertexBuffer;
    
    private static VertexArrayObject _vao;

    internal static void Init() {
        _instanceData = new SpriteInstanceData[InstanceBufferSize];
        _instanceBuffer = new StorageBuffer<SpriteInstanceData>(InstanceBufferSize);

        _vertices = new SpriteVertexData[VertexBufferSize];
        _vertexBuffer = new VertexBuffer<SpriteVertexData>(SpriteVertexData.VertexStride, VertexBufferSize);

        _vao = new VertexArrayObject();
        
        _vertexBuffer.BindTo(_vao);
        
        GL.BindVertexArray(0);
    }

    internal static void StartDraw() {
        _vao.Bind();
        _instanceBuffer.BindToIndex(0);
        
        UiRender.UiShader.Apply();
        
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.StencilTest);

        _instanceCount = 0;
        _vertexCount = 0;
    }

    internal static void Draw(SpriteInstanceData data, ReadOnlySpan<VertexUi> vertices) {
        if (_instanceCount + 1 > InstanceBufferSize ||  _vertexCount + vertices.Length > VertexBufferSize) {
            Flush();
        }
        
        _instanceData[_instanceCount] = data;
        var instanceId = _instanceCount++;
        
        for (var i = 0; i < vertices.Length; i++) {
            _vertices[_vertexCount + i] = new SpriteVertexData(vertices[i], instanceId);
        }
        _vertexCount += vertices.Length;
        
        UiRender.LastRenderCount++;
    }

    internal static void EndDraw() {
        Flush();
        GL.BindVertexArray(0);
    }

    private static void Flush() {
        _instanceBuffer.SetData(_instanceData.AsSpan());
        _vertexBuffer.SetData(_vertices.AsSpan());
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
        
        _instanceCount = 0;
        _vertexCount = 0;
    }
}