using System;

namespace AlloyClient.Networking.Packets.Outgoing;

public class Hello : OutgoingPacket<Hello> {
    public string BuildVersion;
    public int GameId;
    public string Username;
    public string Password;
    public string MapJSON;

    public override PacketId PacketId => PacketId.Hello;

    public override void Reset() {
        BuildVersion = string.Empty;
        GameId = 0;
        Username = string.Empty;
        Password = string.Empty;
        MapJSON = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUTF(BuildVersion);
        writer.Write(GameId);
        writer.WriteUTF(Username);
        writer.WriteUTF(Password);
        writer.Write32UTF(MapJSON);
    }

    public override string ToString() {
        return $"BuildVersion: {BuildVersion}, GameId: {GameId}, GUID: {Username}, Password: {Password}, MapJSON: {MapJSON}";
    }
}