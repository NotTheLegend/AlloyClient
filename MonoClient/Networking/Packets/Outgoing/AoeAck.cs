using MonoClient.Networking.Structs.DataObjects;

namespace MonoClient.Networking.Packets.Outgoing;

public class AoeAck : OutgoingPacket<AoeAck> {
    public int Time;
    public Position Pos;
    public bool Hit;

    public override PacketId PacketId => PacketId.AoeAck;

    public override void Reset() {
        Time = 0;
        Pos.Reset();
        Hit = false;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Time);
        Pos.Write(writer);
        writer.Write(Hit);
    }

    public override string ToString() {
        return $"Time: {Time}, Pos: {Pos}, Hit: {Hit}";
    }
}