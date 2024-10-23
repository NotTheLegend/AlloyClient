namespace MonoClient.Networking.Packets.Outgoing;

public class CloseGame : OutgoingPacket<CloseGame> {
    public override PacketId PacketId => PacketId.CloseGame;

    public override void Reset() {
    }

    public override void Write(NetworkWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}