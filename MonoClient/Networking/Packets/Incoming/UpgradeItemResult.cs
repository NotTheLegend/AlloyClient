namespace MonoClient.Networking.Packets.Incoming;

public class UpgradeItemResult : IncomingPacket<UpgradeItemResult> {
    public int ResultItem;
    public string ErrorText;

    public override PacketId PacketId => PacketId.UpgradeItemResult;

    public override void Reset() {
        ResultItem = 0;
        ErrorText = null;
    }

    public override void Read(NetworkReader reader) {
        ResultItem = reader.ReadInt32();
        ErrorText = reader.ReadUtf();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"ResultItem: {ResultItem}, ErrorText: {ErrorText}";
    }
}