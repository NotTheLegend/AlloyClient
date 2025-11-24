using System;

namespace RealmClient.Networking.Packets.Outgoing;

public class ChangeTrade : OutgoingPacket<ChangeTrade> {
    public bool[] Offers;

    public override PacketId PacketId => PacketId.ChangeTrade;

    public override void Reset() {
        Offers = Array.Empty<bool>();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write((short)Offers.Length);

        foreach (var offer in Offers) {
            writer.Write(offer);
        }
    }

    public override string ToString() {
        return $"Offers: {Offers}";
    }
}