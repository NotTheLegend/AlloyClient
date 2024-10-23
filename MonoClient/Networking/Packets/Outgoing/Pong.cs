namespace MonoClient.Networking.Packets.Outgoing;

public class Pong : OutgoingPacket<Pong> {
    public override PacketId PacketId => PacketId.Pong;

    public override void Reset() {
    }

    public override void Write(NetworkWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}