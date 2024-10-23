namespace MonoClient.Networking.Packets.Incoming;

public class TradeAccepted : IncomingPacket<TradeAccepted> {
    public bool[] MyOffer;
    public bool[] YourOffer;

    public override PacketId PacketId => PacketId.TradeAccepted;

    public override void Reset() {
        MyOffer = null;
        YourOffer = null;
    }

    public override void Read(NetworkReader reader) {
        MyOffer = new bool[reader.ReadInt16()];

        for (var i = 0; i < MyOffer.Length; i++) {
            MyOffer[i] = reader.ReadBoolean();
        }

        YourOffer = new bool[reader.ReadInt16()];

        for (var i = 0; i < YourOffer.Length; i++) {
            YourOffer[i] = reader.ReadBoolean();
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"MyOffer: {MyOffer}, YourOffer: {YourOffer}";
    }
}