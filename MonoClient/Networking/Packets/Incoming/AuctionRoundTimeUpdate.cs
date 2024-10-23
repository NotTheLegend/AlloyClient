namespace MonoClient.Networking.Packets.Incoming;

public class AuctionRoundTimeUpdate : IncomingPacket<AuctionRoundTimeUpdate> {
    public int TimeLeft;
    public int Round;
    public bool AuctionFinished;
    public bool RoundEnd;

    public override PacketId PacketId => PacketId.AuctionRoundTimeUpdate;

    public override void Reset() {
        TimeLeft = 0;
        Round = 0;
        AuctionFinished = false;
        RoundEnd = false;
    }

    public override void Read(NetworkReader reader) {
        TimeLeft = reader.ReadInt32();
        Round = reader.ReadInt32();
        AuctionFinished = reader.ReadBoolean();
        RoundEnd = reader.ReadBoolean();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"TimeLeft: {TimeLeft}, Round: {Round}, AuctionFinished: {AuctionFinished}, RoundEnd: {RoundEnd}";
    }
}