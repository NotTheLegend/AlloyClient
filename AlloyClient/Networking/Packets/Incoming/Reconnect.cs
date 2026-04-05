using AlloyClient.Data;
using AlloyClient.Game;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.State;

namespace AlloyClient.Networking.Packets.Incoming;

public class Reconnect : IncomingPacket<Reconnect> {
    public int GameId;

    public override PacketId PacketId => PacketId.Reconnect;

    public override void Reset() {
        GameId = 0;
    }

    public override void Read(ref SpanReader reader) {
        GameId = reader.ReadInt32();
    }

    public override void Handle() {
        Map.Entities.Clear();
        Map.EntityStorage.Clear();

        var login = GlobalData.Get<LoginData>();
        var hello = Hello.CreatePacket();
        hello.BuildVersion = Settings.BuildVersion;
        hello.GameId = GameId;
        hello.Username = login.Username;
        hello.Password = login.Password;
        hello.MapJSON = "";
        Client.QueuePacket(hello);
    }

    public override string ToString() {
        return $"GameId: {GameId}";
    }
}