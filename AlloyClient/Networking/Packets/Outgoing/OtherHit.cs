namespace AlloyClient.Networking.Packets.Outgoing;

public class OtherHit : OutgoingPacket<OtherHit> {
    public int Time;
    public byte BulletId;
    public int ObjectId;
    public int TargetId;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        Time = 0;
        BulletId = 0;
        ObjectId = 0;
        TargetId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Time);
        writer.Write(BulletId);
        writer.Write(ObjectId);
        writer.Write(TargetId);
    }

    public override string ToString() {
        return $"Time: {Time}, BulletId: {BulletId}, ObjectId: {ObjectId}, TargetId: {TargetId}";
    }
}