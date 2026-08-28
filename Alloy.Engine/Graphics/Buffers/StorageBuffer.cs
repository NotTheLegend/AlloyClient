namespace Alloy.Engine.Graphics.Buffers;

public sealed unsafe class StorageBuffer<T> : IDisposable where T : unmanaged, IBufferData<T> {

    public readonly int Length;
    
    internal int Handle;

    public StorageBuffer(int elementCount) {
        if (sizeof(T) % 16 != 0) throw new Exception("[SSBO] data size not multiple of 16, requirement of (stb140)");

        Length = elementCount;
        
        GL.CreateBuffer(out Handle);
        GL.NamedBufferStorage(Handle, elementCount * sizeof(T), null, BufferStorageMask.DynamicStorageBit);
    }

    public void SetData(ReadOnlySpan<T> data) {
        if (data.Length > Length) {
            throw new Exception("Data larger than buffer");
        }
        
        GL.NamedBufferSubData(Handle, 0, sizeof(T) * data.Length, data);
    }

    public void BindToIndex(uint index) => GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, index, Handle);

    public void Dispose() {
        if (Handle == 0) {
            return;
        }

        GL.DeleteBuffer(Handle);
        Handle = 0;
    }
}
