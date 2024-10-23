using System;

namespace MonoClient.Networking.Packets.Outgoing;

public class ReforgeItem : OutgoingPacket<ReforgeItem> {
    public int[] SlotIds;

    public override PacketId PacketId => PacketId.ReforgeItem;

    public override void Reset() {
        SlotIds = Array.Empty<int>();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)SlotIds.Length);

        foreach (var slotId in SlotIds) {
            writer.Write(slotId);
        }
    }

    public override string ToString() {
        return $"SlotIds: {string.Join(", ", SlotIds)}";
    }
}