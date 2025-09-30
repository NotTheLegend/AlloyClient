using OpenTK.Graphics.OpenGL;

namespace Common.Rendering;

public interface IBufferData<T> : IEquatable<T>;

public sealed unsafe class StorageBuffer<T> where T : unmanaged, IBufferData<T> {
    
    private const BufferTarget Target = BufferTarget.ShaderStorageBuffer;

    private readonly int _size;
    private readonly int _count;
    
    private readonly int _vbo;

    public StorageBuffer(int size, int vertexCount, BufferUsage usage) {
        if (size % 16 != 0) throw new Exception("[SSBO] data size not multiple of 16, requirement of (stb140)");
        
        _size = size;
        _count = vertexCount;

        _vbo = GL.GenBuffer();
        GL.BindBuffer(Target, _vbo);
        GL.BufferData(Target, _count * _size, null, usage);
        GL.BindBufferBase(Target, 0, _vbo);
        GL.BindBuffer(Target, 0);
    }

    public void SetData(T[] data, int dataStart, int dataCount) {
        if (dataCount > _count || dataCount + dataStart > data.Length) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(_vbo, 0, _size * dataCount, new ReadOnlySpan<T>(data, dataStart, dataCount));
    }

    public void SetData(ReadOnlySpan<T> data) {
        if (data.Length > _count) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(_vbo, 0, _size * data.Length, data);
    }
    
    public void SetData(T[] data) {
        if (data.Length > _count) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(_vbo, 0, _size * data.Length, data);
    }

    public void BindToIndex(uint index) => GL.BindBufferBase(Target, index, _vbo);
}