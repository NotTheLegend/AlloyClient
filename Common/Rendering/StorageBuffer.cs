using OpenTK.Graphics.OpenGL;

namespace Common.Rendering;

public interface IBufferData;

public sealed unsafe class StorageBuffer<T> where T : unmanaged, IBufferData {
    
    private const BufferTarget Target = BufferTarget.ShaderStorageBuffer;

    private readonly int _size;
    private readonly int _count;
    
    private readonly int _vbo;

    public StorageBuffer(int size, int vertexCount, BufferUsage usage) {
        if (size % 16 != 0) throw new Exception("[SSBO] data size not multiple of 16, requirement of (stb140)");
        
        _size = size;
        _count = vertexCount;

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _vbo);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, _count * _size, null, usage);
        GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, _vbo);
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void SetData(T[] data, int start, int count) {
        if (count > _count || count + start > data.Length) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.BindBuffer(Target, _vbo);
        GL.BufferSubData(Target, start, _size * count, data);
    }
    
    public void SetData(T[] data) {
        if (data.Length > _count) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.BindBuffer(Target, _vbo);
        GL.BufferSubData(Target, 0, _size * data.Length, data);
    }
    
    public void SetDataOnce(T[] data, int start, int count) {
        SetData(data, start, count);
        UnbindBuffer();
    }
    
    public void SetDataOnce(T[] data) {
        SetData(data);
        UnbindBuffer();
    }

    public void BindToIndex(uint index) => GL.BindBufferBase(Target, index, _vbo);

    public void UnbindBuffer() => GL.BindBuffer(Target, 0);
}