namespace AlloyClient.Networking.Packets.Outgoing;

public class GotoAck : OutgoingPacket<GotoAck> {
    public override PacketId PacketId => PacketId.GotoAck;

    public int Time;

    public override void Reset() {
        Time = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Time);
    }

    public override string ToString() {
        return "";
    }
}