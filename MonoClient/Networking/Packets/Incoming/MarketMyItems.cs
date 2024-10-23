using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Incoming;

public class MarketMyItems : IncomingPacket<MarketMyItems> {
    public MarketItem[] Items;

    public override PacketId PacketId => PacketId.MarketMyItems;

    public override void Reset() {
        Items = null;
    }

    public override void Read(NetworkReader reader) {
        Items = new MarketItem[reader.ReadInt32()];

        for (var i = 0; i < Items.Length; i++) {
            Items[i].Read(reader);
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Items: {Items}";
    }
}