using System;

namespace MonoClient.Networking.Packets.Outgoing;

public class MarketSell : OutgoingPacket<MarketSell> {
    public int[] Offers;
    public int Price;

    public override PacketId PacketId => PacketId.MarketSell;

    public override void Reset() {
        Offers = Array.Empty<int>();
        Price = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)Offers.Length);

        foreach (var offer in Offers) {
            writer.Write(offer);
        }

        writer.Write(Price);
    }

    public override string ToString() {
        return $"Offers: {Offers}, Price: {Price}";
    }
}