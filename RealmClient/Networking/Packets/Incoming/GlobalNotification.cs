namespace RealmClient.Networking.Packets.Incoming;

public class GlobalNotification : IncomingPacket<GlobalNotification> {
    public int Type;
    public string Text;

    public override PacketId PacketId => PacketId.GlobalNotification;

    public override void Reset() {
        Type = 0;
        Text = null;
    }

    public override void Read(NetworkReader reader) {
        Type = reader.ReadInt32();
        Text = reader.ReadUtf();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Type: {Type}, Text: {Text}";
    }
}