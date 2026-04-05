using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class AoeAck : OutgoingPacket<AoeAck> {
    public int Time;
    public Position Pos;

    public override PacketId PacketId => PacketId.AoeAck;

    public override void Reset() {
        Time = 0;
        Pos.Reset();
    }

    public override void Write(ref SpanWriter writer) {
        writer.Write(Time);
        Pos.Write(ref writer);
    }

    public override string ToString() {
        return $"Time: {Time}, Pos: {Pos}";
    }
}