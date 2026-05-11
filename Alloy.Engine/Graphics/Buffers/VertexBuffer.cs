namespace Alloy.Engine.Graphics.Buffers;

public sealed unsafe class VertexBuffer<T> where T : unmanaged, IVertexData<T> {
    
    public readonly int Length;
    
    public readonly int LengthBytes;

    public readonly VertexStride Stride;
    
    internal readonly int Handle;
    
    public VertexBuffer(VertexStride stride, int vertexCount) {
        Length = vertexCount;
        Length = vertexCount * sizeof(T);
        Stride = stride;
        
        GL.CreateBuffer(out Handle);
        GL.NamedBufferStorage(Handle, vertexCount * sizeof(T), null, BufferStorageMask.DynamicStorageBit);
    }
    
    public void SetData(ReadOnlySpan<T> data) {
        if (data.Length > Length) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(Handle, 0, sizeof(T) * data.Length, data);
    }
    
    
    public void BindTo(VertexArrayObject vao, uint index = 0) {
        GL.VertexArrayVertexBuffer(vao.Handle, index, Handle, 0, sizeof(T));
        Stride.BindAttributes(vao, index);
    }

    public void Delete() => GL.DeleteBuffer(Handle);
}