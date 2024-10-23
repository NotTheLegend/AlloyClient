namespace MonoClient.Networking.Packets.Outgoing;

public class ObjectInteract : OutgoingPacket<ObjectInteract> {
    public int ObjectId;

    public override PacketId PacketId => PacketId.ObjectInteract;

    public override void Reset() {
        ObjectId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(ObjectId);
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}";
    }
}