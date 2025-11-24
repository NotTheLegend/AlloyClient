using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Incoming;

public class TradeStart : IncomingPacket<TradeStart> {
    public TradeItem[] MyItems;
    public string YourName;
    public TradeItem[] YourItems;

    public override PacketId PacketId => PacketId.TradeStart;

    public override void Reset() {
        MyItems = null;
        YourName = null;
        YourItems = null;
    }

    public override void Read(NetworkReader reader) {
        MyItems = new TradeItem[reader.ReadInt16()];

        for (var i = 0; i < MyItems.Length; i++) {
            MyItems[i].Read(reader);
        }

        YourName = reader.ReadUTF();

        YourItems = new TradeItem[reader.ReadInt16()];

        for (var i = 0; i < YourItems.Length; i++) {
            YourItems[i].Read(reader);
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"MyItems: {MyItems}, YourName: {YourName}, YourItems: {YourItems}";
    }
}