using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class UseItem : OutgoingPacket<UseItem> {
    public ObjectSlot SlotObject;
    public Position ItemUsePos;
    public byte UseType;
    public bool MassUse;
    public int Time;

    public override PacketId PacketId => PacketId.UseItem;

    public override void Reset() {
        SlotObject.Reset();
        ItemUsePos.Reset();
        UseType = 0;
        MassUse = false;
        Time = 0;
    }

    public override void Write(NetworkWriter writer) {
        SlotObject.Write(writer);
        ItemUsePos.Write(writer);
        writer.Write(UseType);
        writer.Write(MassUse);
        writer.Write(Time);
    }

    public override string ToString() {
        return
            $"SlotObject: {SlotObject}, ItemUsePos: {ItemUsePos}, UseType: {UseType}, MassUse: {MassUse}, Time: {Time}";
    }
}