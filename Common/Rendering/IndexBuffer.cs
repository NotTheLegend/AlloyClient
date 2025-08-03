using OpenTK.Graphics.OpenGL;

namespace Common.Rendering;

public sealed unsafe class IndexBuffer {
    
    private readonly int _count;
    
    private readonly int _ebo;
    private readonly BufferUsage _usage;

    public IndexBuffer(int indexCount, BufferUsage usage) {
        _count = indexCount;
        _usage = usage;

        _ebo = GL.GenBuffer();
    }

    public void SetData(ushort[] data, int start, int count) {
        if (count > _count || count + start > data.Length) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, start, sizeof(ushort) * count, data);
    }
    
    public void SetData(ushort[] data) {
        if (data.Length > _count) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, sizeof(ushort) * data.Length, data);
    }

    public void Bind() {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _count * sizeof(ushort), null, _usage);
    }
}