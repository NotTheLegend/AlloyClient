namespace MonoClient.Networking.Packets.Outgoing;

public class GetAuctionState : OutgoingPacket<GetAuctionState> {
    public bool IsHistoryUpdate;

    public override PacketId PacketId => PacketId.GetAuctionState;

    public override void Reset() {
        IsHistoryUpdate = false;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(IsHistoryUpdate);
    }

    public override string ToString() {
        return $"IsHistoryUpdate: {IsHistoryUpdate}";
    }
}