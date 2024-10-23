namespace MonoClient.Networking.Packets.Incoming;

public class AuctionItemUpdate : IncomingPacket<AuctionItemUpdate> {
    public string ItemObjectId;
    public int StartingBid;

    public override PacketId PacketId => PacketId.AuctionItemUpdate;

    public override void Reset() {
        ItemObjectId = null;
        StartingBid = 0;
    }

    public override void Read(NetworkReader reader) {
        ItemObjectId = reader.ReadUtf();
        StartingBid = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"ItemObjectId: {ItemObjectId}, StartingBid: {StartingBid}";
    }
}