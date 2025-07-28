using Common.Rendering;
using MonoClient.Rendering.VertexData;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MonoClient.Rendering;

public static partial class Render {

    public static int LastDrawParticleCount;

    private const int Buffer = 2000;

    private static int _particleCount;

    private static int _particleVao;
    
    private static IndexBuffer _particleBaseIndexBuffer;
    private static VertexBuffer<VertexBase> _particleBaseVertexBuffer;

    private static VertexParticle[] _particles;
    private static VertexBuffer<VertexParticle> _particleVertexBuffer;

    private static void BuildParticleBuffers() {
        _particleVao = GL.GenVertexArray();
        GL.BindVertexArray(_particleVao);
        
        _particleBaseIndexBuffer = new IndexBuffer(6, BufferUsage.StaticDraw);
        _particleBaseIndexBuffer.Bind();
        _particleBaseIndexBuffer.SetData([0, 1, 2, 0, 2, 3]);

        _particleBaseVertexBuffer = new VertexBuffer<VertexBase>(VertexBase.VertexStride, 4, BufferUsage.StaticDraw);
        _particleBaseVertexBuffer.Bind();
        _particleBaseVertexBuffer.SetData([
            new VertexBase(new Vector3(-0.1f, -0.1f, 0f), new Vector2(0f, 0f)),
            new VertexBase(new Vector3(0.1f, -0.1f, 0f), new Vector2(1f, 0f)),
            new VertexBase(new Vector3(0.1f, 0.1f, 0f), new Vector2(1f, 1f)),
            new VertexBase(new Vector3(-0.1f, 0.1f, 0f), new Vector2(0f, 1f))
        ]);

        _particles = new VertexParticle[Buffer];
        _particleVertexBuffer = new VertexBuffer<VertexParticle>(VertexParticle.VertexStride, Buffer, BufferUsage.DynamicDraw);
        _particleVertexBuffer.Bind();
        _particleVertexBuffer.SetData(_particles);
    }

    public static void DrawParticles(VertexParticle[] particles, int count) {
        if (count < 1) return;
        
        LastDrawParticleCount= 0;
        _particleCount = 0;
        _shaderParticle.Apply();
        GL.BindVertexArray(_particleVao);
        
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
        
        GL.DrawElementsInstanced(PrimitiveType.Triangles, 2 * 3, DrawElementsType.UnsignedShort, 0, count);
    }
    
}