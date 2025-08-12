using RealmClient.Networking.Structs.DataObjects;

namespace RealmClient.Networking.Packets.Outgoing;

public class InvSwap : OutgoingPacket<InvSwap> {
    public int Time;
    public Position Position;
    public ObjectSlot SlotObj1;
    public ObjectSlot SlotObj2;

    public override PacketId PacketId => PacketId.InvSwap;

    public override void Reset() {
        Time = 0;
        Position.Reset();
        SlotObj1.Reset();
        SlotObj2.Reset();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Time);
        Position.Write(writer);
        SlotObj1.Write(writer);
        SlotObj2.Write(writer);
    }

    public override string ToString() {
        return $"SlotObj1: {SlotObj1}, SlotObj2: {SlotObj2}";
    }
}