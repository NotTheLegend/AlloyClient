using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class AoeAck : OutgoingPacket<AoeAck> {
    public int Time;
    public Position Pos;

    public override PacketId PacketId => PacketId.AoeAck;

    public override void Reset() {
        Time = 0;
        Pos.Reset();
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Time);
        Pos.Write(writer);
    }

    public override string ToString() {
        return $"Time: {Time}, Pos: {Pos}";
    }
}