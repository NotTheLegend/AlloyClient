namespace AlloyClient.Networking.Packets.Incoming;

public class TradeRequested : IncomingPacket<TradeRequested> {
    public string Name;

    public override PacketId PacketId => PacketId.TradeRequested;

    public override void Reset() {
        Name = null;
    }

    public override void Read(NetworkReader reader) {
        Name = reader.ReadUTF();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Name: {Name}";
    }
}