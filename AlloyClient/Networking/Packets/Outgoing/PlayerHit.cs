namespace AlloyClient.Networking.Packets.Outgoing;

public class PlayerHit : OutgoingPacket<PlayerHit> {
    public ushort BulletId;
    public int ObjectId;

    public override PacketId PacketId => PacketId.PlayerHit;

    public override void Reset() {
        BulletId = 0;
        ObjectId = 0;
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(BulletId);
        writer.Write(ObjectId);
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, ObjectId: {ObjectId}";
    }
}