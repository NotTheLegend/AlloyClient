namespace MonoClient.Networking.Packets.Outgoing;

public class SatchelStore : OutgoingPacket<SatchelStore> {
    public int ObjectId;

    public override PacketId PacketId => PacketId.SatchelStore;

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