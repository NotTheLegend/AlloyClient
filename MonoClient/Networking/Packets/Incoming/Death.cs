using MonoClient.Display;
using MonoClient.Screens;
using MonoClient.Screens.Title;
using MonoClient.UiLib;

namespace MonoClient.Networking.Packets.Incoming;

public class Death : IncomingPacket<Death> {
    public int AccountId;
    public int CharId;
    public string KilledBy;

    public override PacketId PacketId => PacketId.Death;

    public override void Reset() {
        AccountId = 0;
        CharId = 0;
        KilledBy = string.Empty;
    }

    public override void Read(NetworkReader reader) {
        AccountId = reader.ReadInt32();
        CharId = reader.ReadInt32();
        KilledBy = reader.ReadUtf();
    }

    public override void Handle() {
        ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0);
    }

    public override string ToString() {
        return $"AccountId: {AccountId}, CharId: {CharId}, KilledBy: {KilledBy}";
    }
}