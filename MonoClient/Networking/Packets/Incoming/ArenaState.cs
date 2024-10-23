namespace MonoClient.Networking.Packets.Incoming;

public class ArenaState : IncomingPacket<ArenaState> {
    public string ThemeName;
    public int Stage;
    public int Wave;
    public int BreakTime;

    public override PacketId PacketId => PacketId.ArenaState;

    public override void Reset() {
        ThemeName = null;
        Stage = 0;
        Wave = 0;
        BreakTime = 0;
    }

    public override void Read(NetworkReader reader) {
        ThemeName = reader.ReadUtf();
        Stage = reader.ReadInt32();
        Wave = reader.ReadInt32();
        BreakTime = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"ThemeName: {ThemeName}, Stage: {Stage}, Wave: {Wave}, BreakTime: {BreakTime}";
    }
}