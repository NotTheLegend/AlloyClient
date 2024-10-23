using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.Rendering.VertexData;

namespace MonoClient.Rendering;

public static partial class Render {

    public static int LastDrawParticleCount;

    private const int Buffer = 2000;

    private static int _particleCount;
    
    private static IndexBuffer _particleBaseIndexBuffer;
    private static VertexBuffer _particleBaseVertexBuffer;
    private static VertexBufferBinding _particleBaseVertexBinding;

    private static VertexParticle[] _particles;
    private static VertexBuffer _particleVertexBuffer;
    private static VertexBufferBinding _particleVertexBinding;

    private static void BuildParticleBuffers() {
        _particleBaseIndexBuffer = new IndexBuffer(_graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
        _particleBaseIndexBuffer.SetData(new short[] {0, 1, 2, 0, 2, 3});

        _particleBaseVertexBuffer = new VertexBuffer(_graphics, VertexBase.VertexDeclaration, 4, BufferUsage.WriteOnly);
        _particleBaseVertexBuffer.SetData(new VertexBase[] {
            new VertexBase(new Vector3(-0.1f, -0.1f, 0f), new Vector2(0f, 0f)),
            new VertexBase(new Vector3(0.1f, -0.1f, 0f), new Vector2(1f, 0f)),
            new VertexBase(new Vector3(0.1f, 0.1f, 0f), new Vector2(1f, 1f)),
            new VertexBase(new Vector3(-0.1f, 0.1f, 0f), new Vector2(0f, 1f))
        });
        _particleBaseVertexBinding = new VertexBufferBinding(_particleBaseVertexBuffer);

        _particles = new VertexParticle[Buffer];
        _particleVertexBuffer = new DynamicVertexBuffer(_graphics, VertexParticle.VertexDeclaration, Buffer, BufferUsage.WriteOnly);
        _particleVertexBuffer.SetData(_particles);
        _particleVertexBinding = new VertexBufferBinding(_particleVertexBuffer, 0, 1);
    }

    public static void DrawParticles(VertexParticle[] particles, int count) {
        if (count < 1) return;
        
        LastDrawParticleCount= 0;
        _particleCount = 0;
        _shaderParticle.CurrentTechnique.Passes[0].Apply();
        _graphics.Indices = _particleBaseIndexBuffer;
        _graphics.SetVertexBuffers(_particleBaseVertexBinding, _particleVertexBinding);
        
        var startIndex = 0;
        while (count > Buffer) {
            _particleVertexBuffer.SetData(particles, startIndex, Buffer);
            startIndex += Buffer;
            count -= Buffer;
            FlushBufferParticle(Buffer);
        }
        
        _particleVertexBuffer.SetData(particles, startIndex, count);
        FlushBufferParticle(count);
    }

    private static void FlushBufferParticle(int count) {
        if (count < 1) return;

        LastDrawParticleCount += count;
        
        _graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, count);
    }
    
}