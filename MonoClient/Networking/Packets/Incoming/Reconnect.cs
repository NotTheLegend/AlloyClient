using MonoClient.Networking.Packets.Outgoing;
using MonoClient.State;

namespace MonoClient.Networking.Packets.Incoming;

public class Reconnect : IncomingPacket<Reconnect> {
    public string Name;
    public string Host;
    public int Port;
    public int GameId;
    public byte[] Key;

    public override PacketId PacketId => PacketId.Reconnect;

    public override void Reset() {
        Name = null;
        Host = null;
        Port = 0;
        GameId = 0;
        Key = null;
    }

    public override void Read(NetworkReader reader) {
        Name = reader.ReadUtf();
        Host = reader.ReadUtf();
        Port = reader.ReadInt32();
        GameId = reader.ReadInt32();
        Key = reader.ReadBytes(reader.ReadInt16());
    }

    public override void Handle() {
        Map.Entities.Clear();
        Map.EntityStorage.Clear();

        var hello = Hello.CreatePacket();
        hello.BuildVersion = Settings.BuildVersion;
        hello.GameId = GameId;
        hello.GUID = Rsa.EncryptPublic(Data.Account.Email);
        hello.Password = Rsa.EncryptPublic(Data.Account.Password);
        hello.Key = Key ?? [];
        hello.MapJSON = "";
        hello.Signature = "b5f1afad50dda949c1f4e88b7afb84fb";
        hello.ClientSize = 14569539;
        hello.Platform = "web";
        Client.QueuePacket(hello);
    }

    public override string ToString() {
        return $"Name: {Name}, Host: {Host}, Port: {Port}, GameId: {GameId}";
    }
}