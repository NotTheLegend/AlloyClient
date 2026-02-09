using System;
using AlloyClient.Engine.Graphics.Buffers;
using AlloyClient.Rendering.VertexData;
using OpenTK.Graphics.OpenGL;

namespace AlloyClient.Rendering;

public static partial class Render {

    public static int LastDrawParticleCount;

    private const int Buffer = 2000;
    
    private static StorageBuffer<ParticleData> _particleBuffer;

    private static void BuildParticleBuffers() {
        _particleBuffer = new StorageBuffer<ParticleData>(Buffer);
    }

    public static void DrawParticles(ParticleData[] particles, int count) {
        if (count < 1) return;
        
        LastDrawParticleCount= 0;
        
        _defaultVao.Bind();
        
        _shaderParticle.Apply();
        _particleBuffer.BindToIndex(0);
        
        var startIndex = 0;
        while (count > Buffer) {
            _particleBuffer.SetData(new ReadOnlySpan<ParticleData>(particles, startIndex, Buffer));
            startIndex += Buffer;
            count -= Buffer;
            FlushBufferParticle(Buffer);
        }
        
        _particleBuffer.SetData(new ReadOnlySpan<ParticleData>(particles, startIndex, count));
        FlushBufferParticle(count);
    }

    private static void FlushBufferParticle(int count) {
        if (count < 1) return;

        LastDrawParticleCount += count;
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, count * 6);
    }
    
}