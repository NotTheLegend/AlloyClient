using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Incoming;

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

    public override void Read(ref SpanReader reader) {
        ObjectId = reader.ReadInt32();
        Message = reader.ReadUTF();
        Color.Read(ref reader);
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, Message: {Message}, Color: {Color}";
    }
}