namespace RealmClient.Networking.Packets.Incoming;

public class TradeChanged : IncomingPacket<TradeChanged> {
    public bool[] Offer;

    public override PacketId PacketId => PacketId.TradeChanged;

    public override void Reset() {
        Offer = null;
    }

    public override void Read(NetworkReader reader) {
        Offer = new bool[reader.ReadInt16()];

        for (var i = 0; i < Offer.Length; i++) {
            Offer[i] = reader.ReadBoolean();
        }
    }


    public override void Handle() {
    }

    public override string ToString() {
        return $"Offer: {Offer}";
    }
}