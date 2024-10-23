namespace MonoClient.Networking.Packets.Incoming;

public class DespawnProjectiles : IncomingPacket<DespawnProjectiles> {
    public int OwnerId;

    public override PacketId PacketId => PacketId.DespawnProjectiles;

    public override void Reset() {
        OwnerId = 0;
    }

    public override void Read(NetworkReader reader) {
        OwnerId = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"OwnerId: {OwnerId}";
    }
}