namespace AlloyClient.Networking.Packets.Outgoing;

public class CancelTrade : OutgoingPacket<CancelTrade> {
    public override PacketId PacketId => PacketId.CancelTrade;

    public override void Reset() {
    }

    public override void Write(ref SpanWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}