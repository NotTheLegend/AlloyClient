namespace AlloyClient.Networking.Packets.Outgoing;

public class EnemyHit : OutgoingPacket<EnemyHit> {

    public ushort BulletId;
    public int TargetId;

    public override PacketId PacketId => PacketId.EnemyHit;

    public override void Reset() {
        BulletId = 0;
        TargetId = 0;
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(BulletId);
        writer.Write(TargetId);
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, TargetId: {TargetId}";
    }
}