namespace MonoClient.Networking.Packets.Incoming;

public class SwitchMusic : IncomingPacket<SwitchMusic> {
    public string Music;
    public float FadeTime;

    public override PacketId PacketId => PacketId.SwitchMusic;

    public override void Reset() {
        Music = null;
        FadeTime = 0;
    }

    public override void Read(NetworkReader reader) {
        Music = reader.ReadUtf();
        FadeTime = reader.ReadSingle();
    }

    public override void Handle() {
        Sound.Music.PlayMusic(Music, FadeTime);
    }

    public override string ToString() {
        return $"Music: {Music}, FadeTime: {FadeTime}";
    }
}