namespace AlloyClient.Networking.Packets.Outgoing;

public class PlayerHit : OutgoingPacket<PlayerHit> {
    public int ObjectId;
    public ushort BulletId;

    public override PacketId PacketId => PacketId.PlayerHit;

    public override void Reset() {
        ObjectId = 0;
        BulletId = 0;
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(ObjectId);
        writer.Write(BulletId);
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, ObjectId: {ObjectId}";
    }
}