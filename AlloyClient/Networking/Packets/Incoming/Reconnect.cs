using AlloyClient.Data;
using AlloyClient.Game;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.State;

namespace AlloyClient.Networking.Packets.Incoming;

public class Reconnect : IncomingPacket<Reconnect> {
    public string Name;
    public string Host;
    public int Port;
    public int GameId;
    public int KeyTime;
    public byte[] Key;

    public override PacketId PacketId => PacketId.Reconnect;

    public override void Reset() {
        Name = null;
        Host = null;
        Port = 0;
        GameId = 0;
        KeyTime = 0;
        Key = null;
    }

    public override void Read(NetworkReader reader) {
        Name = reader.ReadUTF();
        Host = reader.ReadUTF();
        Port = reader.ReadInt32();
        GameId = reader.ReadInt32();
        KeyTime = reader.ReadInt32();
        Key = reader.ReadBytes(reader.ReadInt16());
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
        hello.Key = Key ?? [];
        hello.MapJSON = "";
        Client.QueuePacket(hello);
    }

    public override string ToString() {
        return $"Name: {Name}, Host: {Host}, Port: {Port}, GameId: {GameId}";
    }
}