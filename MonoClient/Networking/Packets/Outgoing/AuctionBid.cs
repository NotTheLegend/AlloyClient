namespace MonoClient.Networking.Packets.Outgoing;

public class AuctionBid : OutgoingPacket<AuctionBid> {
    public int BidAmount;

    public override PacketId PacketId => PacketId.AuctionBid;

    public override void Reset() {
        BidAmount = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(BidAmount);
    }

    public override string ToString() {
        return $"BidAmount: {BidAmount}";
    }
}