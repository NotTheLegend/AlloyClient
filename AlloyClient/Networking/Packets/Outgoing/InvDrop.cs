using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class InvDrop : OutgoingPacket<InvDrop> {
    public ObjectSlot SlotObject;

    public override PacketId PacketId => PacketId.InvDrop;

    public override void Reset() {
        SlotObject.Reset();
    }

    public override void Write(ref SpanWriter writer) {
        SlotObject.Write(ref writer);
    }

    public override string ToString() {
        return $"SlotObject: {SlotObject}";
    }
}