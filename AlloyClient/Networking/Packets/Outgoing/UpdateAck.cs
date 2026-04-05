namespace AlloyClient.Networking.Packets.Outgoing;

public class UpdateAck : OutgoingPacket<UpdateAck> {
    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
    }

    public override void Write(ref SpanWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}