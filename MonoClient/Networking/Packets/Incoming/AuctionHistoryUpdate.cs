using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Incoming;

public class AuctionHistoryUpdate : IncomingPacket<AuctionHistoryUpdate> {
    public BidHistoryItem[] BidHistory;

    public override PacketId PacketId => PacketId.AuctionHistoryUpdate;

    public override void Reset() {
        BidHistory = null;
    }

    public override void Read(NetworkReader reader) {
        BidHistory = new BidHistoryItem[reader.ReadInt32()];

        for (var i = 0; i < BidHistory.Length; i++) {
            BidHistory[i].Read(reader);
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"BidHistory: {string.Join(", ", BidHistory)}";
    }
}