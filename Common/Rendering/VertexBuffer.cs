using OpenTK.Graphics.OpenGL;

namespace Common.Rendering;

public interface IVertexData {
    public static VertexStride VertexStride { get; }
}

public sealed unsafe class VertexBuffer<T> where T : unmanaged, IVertexData {

    private readonly VertexStride _stride;
    private readonly int _count;
    
    private readonly int _vbo;
    private readonly BufferUsage _usage;

    public VertexBuffer(VertexStride stride, int vertexCount, BufferUsage usage) {
        _stride = stride;
        _count = vertexCount;
        _usage = usage;

        _vbo = GL.GenBuffer();
    }

    public void SetData(T[] data, int start, int count) {
        if (count > _count || count + start > data.Length) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, start, _stride.Stride * count, data);
    }
    
    public void SetData(T[] data) {
        if (data.Length > _count) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, 0, _stride.Stride * data.Length, data);
    }

    public void Bind() {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _count * _stride.Stride, null, _usage);
        _stride.BindAttributes();
    }
}