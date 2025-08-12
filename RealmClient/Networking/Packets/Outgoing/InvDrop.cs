using RealmClient.Networking.Structs.DataObjects;

namespace RealmClient.Networking.Packets.Outgoing;

public class InvDrop : OutgoingPacket<InvDrop> {
    public ObjectSlot SlotObject;

    public override PacketId PacketId => PacketId.InvDrop;

    public override void Reset() {
        SlotObject.Reset();
    }

    public override void Write(NetworkWriter writer) {
        SlotObject.Write(writer);
    }

    public override string ToString() {
        return $"SlotObject: {SlotObject}";
    }
}