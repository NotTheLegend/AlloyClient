namespace MonoClient.Networking.Structs.DataObjects;

public struct TileData : IDataObject {
    public short X;
    public short Y;
    public ushort Type;

    public void Reset() {
        X = 0;
        Y = 0;
        Type = 0;
    }

    public void Read(NetworkReader reader) {
        X = reader.ReadInt16();
        Y = reader.ReadInt16();
        Type = reader.ReadUInt16();
    }

    public void Write(NetworkWriter writer) {
        writer.Write(X);
        writer.Write(Y);
        writer.Write(Type);
    }

    public override string ToString() {
        return $"X: {X}, Y: {Y}, Type: {Type}";
    }
}