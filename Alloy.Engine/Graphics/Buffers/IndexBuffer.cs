using Alloy.Engine.Diagnostics;

namespace Alloy.Engine.Graphics.Buffers;

public sealed unsafe class IndexBuffer : IDisposable {
    
    public readonly int Length;
    
    public readonly int LengthBytes;
    
    internal int Handle;

    public IndexBuffer(int indicesCount) {
        if (indicesCount < 0) throw new Exception("Element count must be >= 0");
        
        Length = indicesCount;
        LengthBytes = indicesCount * sizeof(ushort);

        GL.CreateBuffer(out Handle);
        GL.NamedBufferStorage(Handle, Length * sizeof(ushort), null, BufferStorageMask.DynamicStorageBit);
    }
    
    public void SetData(ReadOnlySpan<ushort> indices, int startIndex, int count, int bufferElementOffset) {
        if (count > Length - bufferElementOffset) throw new Exception("count & bufferOffset exceeds the length of the buffer");
        if (bufferElementOffset < 0 || bufferElementOffset > Length) throw new Exception("bufferOffset is outside the bounds of the buffer");
        
        var sizeInBytes = sizeof(ushort) * count;
        FrameMetrics.RecordUpload(sizeInBytes);
        GL.NamedBufferSubData(Handle, sizeof(ushort) * bufferElementOffset, sizeInBytes, indices.Slice(startIndex, count));
    }
    
    public void SetData(ReadOnlySpan<ushort> indices) {
        if (indices.Length > Length) throw new Exception("Data larger than buffer");
        
        var sizeInBytes = sizeof(ushort) * indices.Length;
        FrameMetrics.RecordUpload(sizeInBytes);
        GL.NamedBufferSubData(Handle, 0, sizeInBytes, indices);
    }

    public void BindTo(VertexArrayObject vao) => GL.VertexArrayElementBuffer(vao.Handle, Handle);

    public void Delete() {
        Dispose();
    }

    public void Dispose() {
        if (Handle == 0) {
            return;
        }

        GL.DeleteBuffer(Handle);
        Handle = 0;
    }
}
