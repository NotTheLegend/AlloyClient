using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class Move : OutgoingPacket<Move> {
    public Position NewPosition;
    public Position MousePosition;
    public int Time;

    public override PacketId PacketId => PacketId.Move;

    public override void Reset() {
        NewPosition.Reset();
        MousePosition.Reset();
        Time = 0;
    }

    public override void Write(NetworkWriter writer) {
        NewPosition.Write(writer);
        MousePosition.Write(writer);
        writer.Write(Time);
    }

    public override string ToString() {
        return $"NewPosition: {NewPosition}, MousePosition: {MousePosition}, Time: {Time}";
    }
}