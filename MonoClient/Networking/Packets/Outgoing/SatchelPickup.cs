namespace MonoClient.Networking.Packets.Outgoing;

public class SatchelPickup : OutgoingPacket<SatchelPickup> {
    public byte SlotId;

    public override PacketId PacketId => PacketId.SatchelPickup;

    public override void Reset() {
        SlotId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(SlotId);
    }

    public override string ToString() {
        return $"SlotId: {SlotId}";
    }
}