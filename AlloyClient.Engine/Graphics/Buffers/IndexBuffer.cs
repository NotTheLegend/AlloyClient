namespace AlloyClient.Engine.Graphics.Buffers;

public sealed unsafe class IndexBuffer {
    
    public readonly int Length;
    
    public readonly int LengthBytes;
    
    internal readonly int Handle;

    public IndexBuffer(int indexLength) {
        if (indexLength < 0) throw new Exception("Element count must be >= 0");
        
        Length = indexLength;
        LengthBytes = indexLength * sizeof(ushort);

        GL.CreateBuffer(out Handle);
        GL.NamedBufferStorage(Handle, Length * sizeof(ushort), null, BufferStorageMask.DynamicStorageBit);
    }
    
    public void SetData(ReadOnlySpan<ushort> indices, int startIndex, int count, int bufferElementOffset) {
        if (count > Length - bufferElementOffset) throw new Exception("count & bufferOffset exceeds the length of the buffer");
        if (bufferElementOffset < 0 || bufferElementOffset > Length) throw new Exception("bufferOffset is outside the bounds of the buffer");
        
        GL.NamedBufferSubData(Handle, sizeof(ushort) * bufferElementOffset, sizeof(ushort) * count, indices.Slice(startIndex, count));
    }
    
    public void SetData(ReadOnlySpan<ushort> indices) {
        if (indices.Length > Length) throw new Exception("Data larger than buffer");
        
        GL.NamedBufferSubData(Handle, 0, sizeof(ushort) * indices.Length, indices);
    }

    public void BindTo(VertexArrayObject vao) => GL.VertexArrayElementBuffer(vao.Handle, Handle);

    public void Delete() => GL.DeleteBuffer(Handle);
}