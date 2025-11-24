namespace AlloyClient.Networking.Packets.Incoming;

public class GuildResult : IncomingPacket<GuildResult> {
    public bool Success;
    public string Text;

    public override PacketId PacketId => PacketId.GuildResult;

    public override void Reset() {
        Success = false;
        Text = null;
    }

    public override void Read(NetworkReader reader) {
        Success = reader.ReadBoolean();
        Text = reader.ReadUTF();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Success: {Success}, Text: {Text}";
    }
}