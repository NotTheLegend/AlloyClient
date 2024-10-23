using System;

namespace MonoClient.Networking.Packets.Outgoing;

public class MarketBuy : OutgoingPacket<MarketBuy> {
    public int[] Ids;

    public override PacketId PacketId => PacketId.MarketBuy;

    public override void Reset() {
        Ids = Array.Empty<int>();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)Ids.Length);

        foreach (var id in Ids) {
            writer.Write(id);
        }
    }

    public override string ToString() {
        return $"Ids: {Ids}";
    }
}