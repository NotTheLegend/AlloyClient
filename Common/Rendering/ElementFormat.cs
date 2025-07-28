using OpenTK.Graphics.OpenGL;

namespace Common.Rendering;

public enum FormatType {
    Default = 1,
    Color = 1,
    Vector2 = 2,
    Vector3 = 3,
    Vector4 = 4,
}

public readonly struct ElementFormat {

    public readonly int Bytes;

    public readonly uint Location;
    
    public readonly VertexAttribPointerType Type;
    
    public readonly FormatType Format;

    public ElementFormat(uint location, VertexAttribPointerType type, FormatType format = FormatType.Default) {
        Location = location;
        Type = type;
        Format = format;
        Bytes = GetByteCount(type) * (int) format;
    }

    private static int GetByteCount(VertexAttribPointerType type) {
        switch (type) {
            case VertexAttribPointerType.Byte:
            case VertexAttribPointerType.UnsignedByte:
                return 1;
            case VertexAttribPointerType.Short:
            case VertexAttribPointerType.UnsignedShort:
            case VertexAttribPointerType.HalfFloat:
                return 2;
            case VertexAttribPointerType.Int:
            case VertexAttribPointerType.UnsignedInt:
            case VertexAttribPointerType.Float:
                return 4;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}