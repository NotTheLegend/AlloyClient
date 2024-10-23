namespace MonoClient.Networking.Packets.Outgoing;

public class PotStorageUse : OutgoingPacket<PotStorageUse> {
    public PotStorageUseType DetailType;
    public ushort UseValue;

    public override PacketId PacketId => PacketId.PotStorageUse;

    public override void Reset() {
        DetailType = default;
        UseValue = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((byte)DetailType);
        writer.Write(UseValue);
    }

    public override string ToString() {
        return $"DetailType: {DetailType}, UseValue: {UseValue}";
    }
}