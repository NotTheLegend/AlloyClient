namespace AlloyClient.Networking.Packets.Outgoing;

public class EnemyHit : OutgoingPacket<EnemyHit> {

    public int Time;
    public byte BulletId;
    public int TargetId;
    public bool Killed;

    public override PacketId PacketId => PacketId.EnemyHit;

    public override void Reset() {
        Time = 0;
        BulletId = 0;
        TargetId = 0;
        Killed = false;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Time);
        writer.Write(BulletId);
        writer.Write(TargetId);
        writer.Write(Killed);
    }

    public override string ToString() {
        return $"Time: {Time}, BulletId: {BulletId}, TargetId: {TargetId}, Killed: {Killed}";
    }
}