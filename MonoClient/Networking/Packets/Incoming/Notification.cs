using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Incoming;

public class Notification : IncomingPacket<Notification> {
    public int ObjectId;
    public string Message;
    public BGRA Color;
    public bool ItemEffect;

    public override PacketId PacketId => PacketId.Notification;

    public override void Reset() {
        ObjectId = 0;
        Message = null;
        Color.Reset();
        ItemEffect = false;
    }

    public override void Read(NetworkReader reader) {
        ObjectId = reader.ReadInt32();
        Message = reader.ReadUtf();
        Color.Read(reader);
        ItemEffect = reader.ReadBoolean();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, Message: {Message}, Color: {Color}, ItemEffect: {ItemEffect}";
    }
}