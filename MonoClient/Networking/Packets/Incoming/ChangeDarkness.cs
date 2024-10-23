namespace MonoClient.Networking.Packets.Incoming;

public class ChangeDarkness : IncomingPacket<ChangeDarkness> {
    public float Darkness;
    public int FadeTime;

    public override PacketId PacketId => PacketId.ChangeDarkness;

    public override void Reset() {
        Darkness = 0;
        FadeTime = 0;
    }

    public override void Read(NetworkReader reader) {
        Darkness = reader.ReadSingle();
        FadeTime = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Darkness: {Darkness}, FadeTime: {FadeTime}";
    }
}