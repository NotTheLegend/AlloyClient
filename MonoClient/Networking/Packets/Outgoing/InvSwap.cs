using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class InvSwap : OutgoingPacket<InvSwap> {
    public ObjectSlot SlotObj1;
    public ObjectSlot SlotObj2;

    public override PacketId PacketId => PacketId.InvSwap;

    public override void Reset() {
        SlotObj1.Reset();
        SlotObj2.Reset();
    }

    public override void Write(NetworkWriter writer) {
        SlotObj1.Write(writer);
        SlotObj2.Write(writer);
    }

    public override string ToString() {
        return $"SlotObj1: {SlotObj1}, SlotObj2: {SlotObj2}";
    }
}