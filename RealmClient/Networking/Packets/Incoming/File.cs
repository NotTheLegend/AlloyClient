namespace RealmClient.Networking.Packets.Incoming;

public class File : IncomingPacket<File> {
    public string Name;
    public byte[] Bytes;

    public override PacketId PacketId => PacketId.File;

    public override void Reset() {
        Name = null;
        Bytes = null;
    }

    public override void Read(NetworkReader reader) {
        Name = reader.ReadUtf();
        var bytesLen = reader.ReadInt32();

        for (var i = 0; i < bytesLen; i++) {
            Bytes[i] = reader.ReadByte();
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Name: {Name}, Bytes: {Bytes}";
    }
}