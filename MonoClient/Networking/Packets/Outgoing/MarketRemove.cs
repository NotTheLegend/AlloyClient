namespace MonoClient.Networking.Packets.Outgoing;

public class MarketRemove : OutgoingPacket<MarketRemove> {
    public int Id;

    public override PacketId PacketId => PacketId.MarketRemove;

    public override void Reset() {
        Id = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Id);
    }

    public override string ToString() {
        return $"Id: {Id}";
    }
}