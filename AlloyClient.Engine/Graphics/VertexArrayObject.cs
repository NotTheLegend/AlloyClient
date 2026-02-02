namespace AlloyClient.Engine.Graphics;

public sealed class VertexArrayObject {

    internal readonly int Handle;

    public VertexArrayObject() => Handle = GL.CreateVertexArray();

    public void Bind() => GL.BindVertexArray(Handle);

    public void Dispose() => GL.DeleteBuffer(Handle);
}