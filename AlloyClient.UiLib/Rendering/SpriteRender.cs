using System;
using Common.Rendering;
using OpenTK.Graphics.OpenGL;

namespace RealmClient.UiLib.Rendering;

public static class SpriteRender {

    private const int InstanceBufferSize = 1000;
    private const int IndexBufferSize = InstanceBufferSize * 6; // Most sprites are a quad which has 6 indices
    private const int VertexBufferSize = InstanceBufferSize * 4; // Most sprites are a quad which has 4 vertices

    private static ushort _instanceCount;
    private static SpriteInstanceData[] _instanceData; 
    private static StorageBuffer<SpriteInstanceData> _instanceBuffer;
    
    private static int _indexCount;
    private static ushort[] _indices;
    private static IndexBuffer _indexBuffer;
    
    private static ushort _vertexCount;
    private static SpriteVertexData[] _vertices;
    private static VertexBuffer<SpriteVertexData> _vertexBuffer;
    
    private static int _vao;

    internal static void Init() {
        _instanceData = new SpriteInstanceData[InstanceBufferSize];
        _instanceBuffer = new StorageBuffer<SpriteInstanceData>(SpriteInstanceData.Size, InstanceBufferSize, BufferUsage.DynamicDraw);

        _indices = new ushort[IndexBufferSize];
        _indexBuffer = new IndexBuffer(IndexBufferSize, BufferUsage.DynamicDraw);

        _vertices = new SpriteVertexData[VertexBufferSize];
        _vertexBuffer = new VertexBuffer<SpriteVertexData>(SpriteVertexData.VertexStride, VertexBufferSize, BufferUsage.DynamicDraw);
        
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        
        _vertexBuffer.Bind();
        _indexBuffer.Bind();
        
        GL.BindVertexArray(0);
    }

    internal static void StartDraw() {
        GL.BindVertexArray(_vao);
        _instanceBuffer.BindToIndex(0);
        
        UiRender.UiShader.Apply();
        
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.StencilTest);

        _instanceCount = 0;
        _indexCount = 0;
        _vertexCount = 0;
    }

    internal static void Draw(SpriteInstanceData data, ReadOnlySpan<ushort> indices, ReadOnlySpan<VertexUi> vertices) {
        if (_instanceCount + 1 > InstanceBufferSize || _indexCount + indices.Length > IndexBufferSize || _vertexCount + vertices.Length > VertexBufferSize)
            Flush();
        
        _instanceData[_instanceCount] = data;
        var instanceId = _instanceCount++;
        var numVertices = (ushort)0;

        var len = indices.Length;
        for (var i = 0; i < len; i++) {
            _indices[_indexCount + i] = (ushort)(_vertexCount + indices[i]);
            numVertices = Math.Max(indices[i], numVertices);// Get highest vertex index
        }
        _indexCount += len;

        numVertices++;
        for (var i = 0; i < numVertices; i++) {
            _vertices[_vertexCount + i] = new SpriteVertexData(vertices[i], instanceId);
        }
        _vertexCount += numVertices;
        
        UiRender.LastRenderCount++;
    }

    internal static void EndDraw() {
        Flush();
        GL.BindVertexArray(0);
    }

    private static void Flush() {
        _instanceBuffer.SetData(_instanceData.AsSpan());
        _indexBuffer.SetData(_indices.AsSpan());
        _vertexBuffer.SetData(_vertices.AsSpan());
        
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedShort, 0);
        
        _instanceCount = 0;
        _indexCount = 0;
        _vertexCount = 0;
    }
}