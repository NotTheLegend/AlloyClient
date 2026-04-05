using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class UseItem : OutgoingPacket<UseItem> {

    public int Time;
    public ObjectSlot SlotObject;
    public Position ItemUsePos;
    public byte UseType;

    public override PacketId PacketId => PacketId.UseItem;

    public override void Reset() {
        SlotObject.Reset();
        ItemUsePos.Reset();
        UseType = 0;
        Time = 0;
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(Time);
        SlotObject.Write(ref writer);
        ItemUsePos.Write(ref writer);
        writer.Write(UseType);
    }

    public override string ToString() {
        return
            $"SlotObject: {SlotObject}, ItemUsePos: {ItemUsePos}, UseType: {UseType}, Time: {Time}";
    }
}