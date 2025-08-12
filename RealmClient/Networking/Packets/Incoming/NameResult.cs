namespace RealmClient.Networking.Packets.Incoming;

public class NameResult : IncomingPacket<NameResult> {
    public bool Success;
    public string ErrorText;

    public override PacketId PacketId => PacketId.NameResult;

    public override void Reset() {
        Success = false;
        ErrorText = null;
    }

    public override void Read(NetworkReader reader) {
        Success = reader.ReadBoolean();
        ErrorText = reader.ReadUtf();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Success: {Success}, ErrorText: {ErrorText}";
    }
}