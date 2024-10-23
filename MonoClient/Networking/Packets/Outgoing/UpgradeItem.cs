using System;

namespace MonoClient.Networking.Packets.Outgoing;

public class UpgradeItem : OutgoingPacket<UpgradeItem> {
    public int[] SlotIds;
    public bool InventoryInteraction;

    public override PacketId PacketId => PacketId.UpgradeItem;

    public override void Reset() {
        SlotIds = Array.Empty<int>();
        InventoryInteraction = false;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)SlotIds.Length);

        foreach (var slotId in SlotIds) {
            writer.Write(slotId);
        }

        writer.Write(InventoryInteraction);
    }

    public override string ToString() {
        return $"SlotIds: {string.Join(", ", SlotIds)}, InventoryInteraction: {InventoryInteraction}";
    }
}