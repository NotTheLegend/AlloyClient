using OpenTK.Graphics.OpenGL;

namespace Common.Rendering;

public sealed unsafe class IndexBuffer {
    
    private readonly int _count;
    
    private readonly int _ebo;

    public IndexBuffer(int indexCount, BufferUsage usage) {
        _count = indexCount;

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _count * sizeof(ushort), null, usage);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void SetData(ushort[] data, int start, int count) {
        if (count > _count || count + start > data.Length) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(_ebo, sizeof(ushort) * start, sizeof(ushort) * count, data);
    }
    
    public void SetData(ReadOnlySpan<ushort> data) {
        if (data.Length > _count) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(_ebo, 0, sizeof(ushort) * data.Length, data);
    }
    
    public void SetData(ushort[] data) {
        if (data.Length > _count) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(_ebo, 0, sizeof(ushort) * data.Length, data);
    }

    public void Bind() {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
    }
}