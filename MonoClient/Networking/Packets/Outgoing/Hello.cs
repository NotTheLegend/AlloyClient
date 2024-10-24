using System;

namespace MonoClient.Networking.Packets.Outgoing;

public class Hello : OutgoingPacket<Hello> {
    public string BuildVersion;
    public int GameId;
    public string GUID;
    public string Password;
    public int KeyTime;
    public byte[] Key;
    public string MapJSON;

    public override PacketId PacketId => PacketId.Hello;

    public override void Reset() {
        BuildVersion = string.Empty;
        GameId = 0;
        GUID = string.Empty;
        Password = string.Empty;
        KeyTime = 0;
        Key = Array.Empty<byte>();
        MapJSON = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUtf(BuildVersion);
        writer.Write(GameId);
        writer.WriteUtf(GUID);
        writer.WriteUtf(Password);
        writer.Write(KeyTime);
        writer.Write((short)Key.Length);
        writer.Write(Key);
        writer.Write32Utf(MapJSON);
    }

    public override string ToString() {
        return $"BuildVersion: {BuildVersion}, GameId: {GameId}, GUID: {GUID}, Password: {Password}, KeyTime: {KeyTime} Key: {Key}, MapJSON: {MapJSON}";
    }
}