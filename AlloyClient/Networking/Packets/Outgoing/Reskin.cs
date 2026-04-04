namespace AlloyClient.Networking.Packets.Outgoing;

public class Reskin : OutgoingPacket<Reskin> {
    public int SkinId;

    public override PacketId PacketId => PacketId.Reskin;

    public override void Reset() {
        SkinId = 0;
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(SkinId);
    }

    public override string ToString() {
        return $"SkinId: {SkinId}";
    }
}