namespace MonoClient.Networking.Packets.Outgoing;

public class ForgeCombine : OutgoingPacket<ForgeCombine> {
    public int ItemId;

    public override PacketId PacketId => PacketId.ForgeCombine;

    public override void Reset() {
        ItemId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(ItemId);
    }

    public override string ToString() {
        return $"ItemId: {ItemId}";
    }
}