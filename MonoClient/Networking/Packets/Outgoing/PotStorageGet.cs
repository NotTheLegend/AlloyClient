namespace MonoClient.Networking.Packets.Outgoing;

public class PotStorageGet : OutgoingPacket<PotStorageGet> {
    public PotStorageDetailType DetailType;

    public override PacketId PacketId => PacketId.PotStorageGet;

    public override void Reset() {
        DetailType = default;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((byte)DetailType);
    }
}