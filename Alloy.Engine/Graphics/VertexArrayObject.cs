namespace Alloy.Engine.Graphics;

public sealed class VertexArrayObject : IDisposable {

    internal int Handle = GL.CreateVertexArray();

    public void Bind() => GL.BindVertexArray(Handle);

    public void Dispose() {
        if (Handle == 0) {
            return;
        }

        GL.DeleteVertexArray(Handle);
        Handle = 0;
    }
}
