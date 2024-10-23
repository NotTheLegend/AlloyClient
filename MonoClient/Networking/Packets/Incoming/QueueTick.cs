namespace MonoClient.Networking.Packets.Incoming;

public class QueueTick : IncomingPacket<QueueTick> {
    public int Position;
    public int TotalQueue;

    public override PacketId PacketId => PacketId.QueueTick;

    public override void Reset() {
        Position = 0;
        TotalQueue = 0;
    }

    public override void Read(NetworkReader reader) {
        Position = reader.ReadInt32();
        TotalQueue = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Position: {Position}, TotalQueue: {TotalQueue}";
    }
}