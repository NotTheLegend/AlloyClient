namespace AlloyClient.Networking.Structs.DataObjects;

public struct ARGB(uint bgra) : IDataObject {
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

    public void Read(ref SpanReader reader) {
        A = reader.ReadByte();
        R = reader.ReadByte();
        G = reader.ReadByte();
        B = reader.ReadByte();
    }

    public void Write(ref SpanWriter writer) {
        writer.Write(A);
        writer.Write(R);
        writer.Write(G);
        writer.Write(B);
    }

    public override string ToString() {
        return $"B: {B}, G: {G}, R: {R}, A: {A}";
    }
}