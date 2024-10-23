namespace MonoClient.Networking.Packets.Outgoing;

public class MarketMyItemsRequest : OutgoingPacket<MarketMyItemsRequest> {
    public override PacketId PacketId => PacketId.MarketMyItemsRequest;

    public override void Write(NetworkWriter writer) {
    }

    public override void Reset() {
    }

    public override string ToString() {
        return "";
    }
}