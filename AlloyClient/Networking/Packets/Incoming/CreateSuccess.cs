using AlloyClient.Game;

namespace AlloyClient.Networking.Packets.Incoming;

public class CreateSuccess : IncomingPacket<CreateSuccess> {
    public int ObjectId;
    public int CharId;

    public override PacketId PacketId => PacketId.CreateSuccess;

    public override void Reset() {
        ObjectId = 0;
        CharId = 0;
    }

    public override void Read(ref SpanReader reader) {
        ObjectId = reader.ReadInt32();
        CharId = reader.ReadInt32();
    }

    public override void Handle() {
        Map.LocalPlayerId = ObjectId;
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, CharId: {CharId}";
    }
}