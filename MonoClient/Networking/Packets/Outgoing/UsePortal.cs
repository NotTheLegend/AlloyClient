namespace MonoClient.Networking.Packets.Outgoing;

public class UsePortal : OutgoingPacket<UsePortal> {
    public int ObjectId;

    public override PacketId PacketId => PacketId.UsePortal;

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