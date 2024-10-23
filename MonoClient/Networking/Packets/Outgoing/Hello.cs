using System;

namespace MonoClient.Networking.Packets.Outgoing;

public class Hello : OutgoingPacket<Hello> {
    public string BuildVersion;
    public int GameId;
    public string GUID;
    public string Password;
    public byte[] Key;
    public string MapJSON;
    public string Signature;
    public int ClientSize;
    public string Platform;

    public override PacketId PacketId => PacketId.Hello;

    public override void Reset() {
        BuildVersion = string.Empty;
        GameId = 0;
        GUID = string.Empty;
        Password = string.Empty;
        Key = Array.Empty<byte>();
        MapJSON = string.Empty;
        Signature = string.Empty;
        ClientSize = 0;
        Platform = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUtf(BuildVersion);
        writer.Write(GameId);
        writer.WriteUtf(GUID);
        writer.WriteUtf(Password);
        writer.Write((short)Key.Length);
        writer.Write(Key);
        writer.WriteUtf32(MapJSON);
        writer.WriteUtf(Signature);
        writer.Write(ClientSize);
        writer.WriteUtf(Platform);
    }

    public override string ToString() {
        return
            $"BuildVersion: {BuildVersion}, GameId: {GameId}, GUID: {GUID}, Password: {Password}, Key: {Key}, MapJSON: {MapJSON}, Signature: {Signature}, ClientSize: {ClientSize}, Platform: {Platform}";
    }
}