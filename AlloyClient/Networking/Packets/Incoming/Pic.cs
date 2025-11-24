namespace AlloyClient.Networking.Packets.Incoming;

public class Pic : IncomingPacket<Pic> {
    public int Width;
    public int Height;
    public byte[] Bytes;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        Width = 0;
        Height = 0;
        Bytes = null;
    }

    public override void Read(NetworkReader reader) {
        Width = reader.ReadInt32();
        Height = reader.ReadInt32();
        Bytes = reader.ReadBytes(Width * Height * 4);
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Width: {Width}, Height: {Height}, Bytes: {Bytes}";
    }
}