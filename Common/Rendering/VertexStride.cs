using OpenTK.Graphics.OpenGL;

namespace Common.Rendering;

public readonly struct VertexStride {

    public readonly int Stride;

    public readonly bool Instanced;

    public readonly ElementFormat[] Layout;

    public VertexStride(ElementFormat[] layout, bool instanced = false) {
        Stride = GetStride(layout);
        Layout = layout;
        Instanced = instanced;
    }

    public void BindAttributes() {
        var offset = 0;
        for (var i = 0u; i < Layout.Length; i++) {
            var e = Layout[i];
            
            GL.EnableVertexAttribArray(e.Location);
            GL.VertexAttribPointer(e.Location, (int)e.Format, e.Type, false, Stride, offset);
            
            if (Instanced)
                GL.VertexAttribDivisor(e.Location, 1);
            
            offset += e.Bytes;
        }
    }

    private static int GetStride(ElementFormat[] layout) {
        var stride = 0;
        foreach (var e in layout) {
            stride += e.Bytes;
        }

        return stride;
    }
    
}