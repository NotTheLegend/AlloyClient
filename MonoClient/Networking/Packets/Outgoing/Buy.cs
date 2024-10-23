namespace MonoClient.Networking.Packets.Outgoing;

public class Buy : OutgoingPacket<Buy> {
    public int ObjectId;
    public int Quantity;

    public override PacketId PacketId => PacketId.Buy;

    public override void Reset() {
        ObjectId = 0;
        Quantity = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(ObjectId);
        writer.Write(Quantity);
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, Quantity: {Quantity}";
    }
}