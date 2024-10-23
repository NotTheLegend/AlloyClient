namespace MonoClient.Networking.Packets.Outgoing;

public class GotoAck : OutgoingPacket<GotoAck> {
    public override PacketId PacketId => PacketId.GotoAck;

    public override void Reset() {
    }

    public override void Write(NetworkWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}