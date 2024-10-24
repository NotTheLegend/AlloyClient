using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Incoming;

public class Notification : IncomingPacket<Notification> {
    public int ObjectId;
    public string Message;
    public ARGB Color;

    public override PacketId PacketId => PacketId.Notification;

    public override void Reset() {
        ObjectId = 0;
        Message = null;
        Color.Reset();
    }

    public override void Read(NetworkReader reader) {
        ObjectId = reader.ReadInt32();
        Message = reader.ReadUtf();
        Color.Read(reader);
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, Message: {Message}, Color: {Color}";
    }
}