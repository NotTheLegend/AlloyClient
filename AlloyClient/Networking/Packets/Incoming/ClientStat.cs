namespace AlloyClient.Networking.Packets.Incoming;

public class ClientStat : IncomingPacket<ClientStat> {
    public string Name;
    public int Value;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        Name = null;
        Value = 0;
    }

    public override void Read(NetworkReader reader) {
        Name = reader.ReadUTF();
        Value = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Name: {Name}, Value: {Value}";
    }
}