namespace Alloy.Engine.Graphics.Buffers;

public sealed unsafe class UniformBuffer {
    
    public readonly int LengthBytes;
    
    internal readonly int Handle;

    public UniformBuffer(int sizeInBytes) {
        if ((sizeInBytes - 1) > ushort.MaxValue) {
            throw new Exception($"Size too big for uniform buffer object, {sizeInBytes}/{ushort.MaxValue}");
        }
        
        LengthBytes = sizeInBytes;
        
        GL.CreateBuffer(out Handle);
        GL.NamedBufferStorage(Handle, sizeInBytes, null, BufferStorageMask.DynamicStorageBit);
    }
    
    public void SetData<T1>(ReadOnlySpan<T1> data, int offsetInBytes) where T1: unmanaged {
        var size = sizeof(T1) * data.Length;
        if (size + offsetInBytes > LengthBytes) throw new Exception("Data larger than buffer");
        
        GL.NamedBufferSubData(Handle, offsetInBytes, size, data);
    }

    public void Bind(Shader shader, string uniform) => GL.BindBufferBase(BufferTarget.UniformBuffer, shader.GetUniformBlock(uniform), Handle);
}