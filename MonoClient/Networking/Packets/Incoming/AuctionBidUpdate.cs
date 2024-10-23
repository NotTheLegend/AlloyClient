namespace MonoClient.Networking.Packets.Incoming;

public class AuctionBidUpdate : IncomingPacket<AuctionBidUpdate> {
    public int YourBid = -1;
    public int HighestBid;
    public string HighestBidPlayer;

    public override PacketId PacketId => PacketId.AuctionBidUpdate;

    public override void Reset() {
        YourBid = -1;
        HighestBid = 0;
        HighestBidPlayer = null;
    }

    public override void Read(NetworkReader reader) {
        YourBid = reader.ReadInt32();
        HighestBid = reader.ReadInt32();
        HighestBidPlayer = reader.ReadUtf();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"YourBid: {YourBid}, HighestBid: {HighestBid}, HighestBidPlayer: {HighestBidPlayer}";
    }
}