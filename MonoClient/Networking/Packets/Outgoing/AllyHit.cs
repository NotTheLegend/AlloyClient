namespace MonoClient.Networking.Packets.Outgoing;

public class AllyHit : OutgoingPacket<AllyHit> {
    public byte BulletId;
    public int OwnerId;
    public int AllyId;

    public override PacketId PacketId => PacketId.AllyHit;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        AllyId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(BulletId);
        writer.Write(OwnerId);
        writer.Write(AllyId);
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, OwnerId: {OwnerId}, AllyId: {AllyId}";
    }
}