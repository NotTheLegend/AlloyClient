using System;
using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class MarketShopRequest : OutgoingPacket<MarketShopRequest> {
    public MarketItem[] Cart;

    public override PacketId PacketId => PacketId.MarketShopRequest;

    public override void Reset() {
        Cart = Array.Empty<MarketItem>();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)Cart.Length);
        foreach (var item in Cart)
            item.Write(writer);
    }

    public override string ToString() {
        return $"Cart: {Cart}";
    }
}