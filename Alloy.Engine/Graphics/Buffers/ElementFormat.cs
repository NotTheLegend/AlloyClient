namespace Alloy.Engine.Graphics.Buffers;

public enum FormatType {
    Default = 1,
    Color = 1,
    Vector2 = 2,
    Vector3 = 3,
    Vector4 = 4,
}

public readonly struct ElementFormat {

    public readonly uint Bytes;

    public readonly uint Location;
    
    public readonly VertexAttribType Type;
    
    public readonly FormatType Format;

    public ElementFormat(uint location, VertexAttribType type, FormatType format = FormatType.Default) {
        Location = location;
        Type = type;
        Format = format;
        Bytes = GetByteCount(type) * (uint)format;
    }

    private static uint GetByteCount(VertexAttribType type) {
        switch (type) {
            case VertexAttribType.Byte:
            case VertexAttribType.UnsignedByte:
                return 1;
            case VertexAttribType.Short:
            case VertexAttribType.UnsignedShort:
            case VertexAttribType.HalfFloat:
                return 2;
            case VertexAttribType.Int:
            case VertexAttribType.UnsignedInt:
            case VertexAttribType.Float:
                return 4;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}