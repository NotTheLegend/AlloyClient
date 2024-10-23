namespace MonoClient.Networking.Packets.Incoming;

public class FadePlayer : IncomingPacket<FadePlayer> {
    public float FadeTime;
    public float FadeDelay;
    public float FadeOutTime;

    public override PacketId PacketId => PacketId.FadePlayer;

    public override void Reset() {
        FadeTime = 0;
        FadeDelay = 0;
        FadeOutTime = 0;
    }

    public override void Read(NetworkReader reader) {
        FadeTime = reader.ReadSingle();
        FadeDelay = reader.ReadSingle();
        FadeOutTime = reader.ReadSingle();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"FadeTime: {FadeTime}, FadeDelay: {FadeDelay}, FadeOutTime: {FadeOutTime}";
    }
}