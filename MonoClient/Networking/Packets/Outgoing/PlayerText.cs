namespace MonoClient.Networking.Packets.Outgoing;

public class PlayerText : OutgoingPacket<PlayerText> {
    public string Text;

    public override PacketId PacketId => PacketId.PlayerText;

    public override void Reset() {
        Text = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUtf(Text);
    }

    public override string ToString() {
        return $"Text: {Text}";
    }
}