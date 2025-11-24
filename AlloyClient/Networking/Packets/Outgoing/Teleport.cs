namespace AlloyClient.Networking.Packets.Outgoing;

public class Teleport : OutgoingPacket<Teleport> {
    public int ObjectId;

    public override PacketId PacketId => PacketId.Teleport;

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