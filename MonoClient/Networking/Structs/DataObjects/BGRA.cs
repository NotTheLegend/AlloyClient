namespace MonoClient.Networking.Structs.DataObjects;

public struct BGRA(uint bgra) : IDataObject {
    public byte B = (byte)(bgra & 0xFF);
    public byte G = (byte)(bgra >> 8 & 0xFF);
    public byte R = (byte)(bgra >> 16 & 0xFF);
    public byte A = (byte)(bgra >> 24 & 0xFF);

    public void Reset() {
        B = 0;
        G = 0;
        R = 0;
        A = 0;
    }

    public void Read(NetworkReader reader) {
        B = reader.ReadByte();
        G = reader.ReadByte();
        R = reader.ReadByte();
        A = reader.ReadByte();
    }

    public void Write(NetworkWriter writer) {
        writer.Write(B);
        writer.Write(G);
        writer.Write(R);
        writer.Write(A);
    }

    public override string ToString() {
        return $"B: {B}, G: {G}, R: {R}, A: {A}";
    }
}