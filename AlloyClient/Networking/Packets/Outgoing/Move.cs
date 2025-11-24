using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class Move : OutgoingPacket<Move> {

    public Position NewPosition;

    public override PacketId PacketId => PacketId.Move;

    public override void Reset() {
        NewPosition.Reset();
    }

    public override void Write(NetworkWriter writer) {
        NewPosition.Write(writer);
    }

    public override string ToString() {
        return $"NewPosition: {NewPosition}";
    }
}