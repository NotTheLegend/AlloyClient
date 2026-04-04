namespace AlloyClient.Networking.Packets.Outgoing;

public class Load : OutgoingPacket<Load> {
    public int CharId;

    public override PacketId PacketId => PacketId.Load;

    public override void Reset() {
        CharId = 0;
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(CharId);
    }

    public override string ToString() {
        return $"CharId: {CharId}";
    }
}