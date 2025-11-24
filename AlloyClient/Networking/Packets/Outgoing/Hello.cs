using System;

namespace AlloyClient.Networking.Packets.Outgoing;

public class Hello : OutgoingPacket<Hello> {
    public string BuildVersion;
    public int GameId;
    public string Username;
    public string Password;
    public int KeyTime;
    public byte[] Key;
    public string MapJSON;

    public override PacketId PacketId => PacketId.Hello;

    public override void Reset() {
        BuildVersion = string.Empty;
        GameId = 0;
        Username = string.Empty;
        Password = string.Empty;
        KeyTime = 0;
        Key = Array.Empty<byte>();
        MapJSON = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUTF(BuildVersion);
        writer.Write(GameId);
        writer.WriteUTF(Username);
        writer.WriteUTF(Password);
        writer.Write(KeyTime);
        writer.Write((short)Key.Length);
        writer.Write(Key);
        writer.Write32UTF(MapJSON);
    }

    public override string ToString() {
        return $"BuildVersion: {BuildVersion}, GameId: {GameId}, GUID: {Username}, Password: {Password}, KeyTime: {KeyTime} Key: {Key}, MapJSON: {MapJSON}";
    }
}