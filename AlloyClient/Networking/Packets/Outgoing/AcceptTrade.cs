using System;

namespace AlloyClient.Networking.Packets.Outgoing;

public class AcceptTrade : OutgoingPacket<AcceptTrade> {
    public bool[] MyOffer;
    public bool[] YourOffer;

    public override PacketId PacketId => PacketId.AcceptTrade;

    public override void Reset() {
        MyOffer = Array.Empty<bool>();
        YourOffer = Array.Empty<bool>();
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write((short)MyOffer.Length);

        foreach (var b in MyOffer) {
            writer.Write(b);
        }

        writer.Write((short)YourOffer.Length);

        foreach (var b in YourOffer) {
            writer.Write(b);
        }
    }

    public override string ToString() {
        return $"MyOffer: {MyOffer}, YourOffer: {YourOffer}";
    }
}