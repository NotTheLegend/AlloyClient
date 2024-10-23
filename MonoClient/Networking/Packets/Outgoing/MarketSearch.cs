using System;
using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class MarketSearch : OutgoingPacket<MarketSearch> {
    public int Item;
    public MarketItem[] Cart;

    public override PacketId PacketId => PacketId.MarketSearch;

    public override void Reset() {
        Item = 0;
        Cart = Array.Empty<MarketItem>();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Item);

        writer.Write((short)Cart.Length);
        foreach (var item in Cart)
            item.Write(writer);
    }

    public override string ToString() {
        return $"Item: {Item}, Cart: {Cart}";
    }
}