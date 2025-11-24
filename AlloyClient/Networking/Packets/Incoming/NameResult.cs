namespace AlloyClient.Networking.Packets.Incoming;

public class NameResult : IncomingPacket<NameResult> {
    public bool Success;
    public string ErrorText;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        Success = false;
        ErrorText = null;
    }

    public override void Read(NetworkReader reader) {
        Success = reader.ReadBoolean();
        ErrorText = reader.ReadUTF();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Success: {Success}, ErrorText: {ErrorText}";
    }
}