namespace AlloyClient.Networking.Packets.Outgoing;

public class Escape : OutgoingPacket<Escape> {
    public override PacketId PacketId => PacketId.Escape;

    public override void Reset() {
    }

    public override void Write(ref SpanWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}