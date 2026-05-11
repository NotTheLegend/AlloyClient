namespace Alloy.Engine.Graphics.Buffers;

public readonly struct VertexStride {

    public readonly uint Stride;

    public readonly bool Instanced;

    public readonly ElementFormat[] Layout;

    public VertexStride(ElementFormat[] layout, bool instanced = false) {
        Stride = (uint)layout.Sum(e => e.Bytes);
        Layout = layout;
        Instanced = instanced;
    }

    public void BindAttributes(VertexArrayObject vao, uint index) {
        var offset = 0u;
        for (var i = 0u; i < Layout.Length; i++) {
            var e = Layout[i];
            
            GL.EnableVertexArrayAttrib(vao.Handle, e.Location);
            GL.VertexArrayAttribBinding(vao.Handle, e.Location, index);
            
            if (Instanced) {
                GL.VertexArrayBindingDivisor(vao.Handle, index, 1);
            }

            switch (e.Type) {
                case VertexAttribType.Byte:
                case VertexAttribType.UnsignedByte:
                case VertexAttribType.Short:
                case VertexAttribType.UnsignedShort:
                case VertexAttribType.Int:
                case VertexAttribType.UnsignedInt:
                    GL.VertexArrayAttribIFormat(vao.Handle, e.Location, (int)e.Format, (VertexAttribIType)e.Type, offset);
                    break;
                case VertexAttribType.Float:
                case VertexAttribType.HalfFloat:
                    GL.VertexArrayAttribFormat(vao.Handle, e.Location, (int)e.Format, e.Type, false, offset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e.Type), e.Type, null);
            }
            
            offset += e.Bytes;
        }
    }
}