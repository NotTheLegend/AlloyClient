namespace RealmClient.Networking.Packets.Outgoing;

public class Escape : OutgoingPacket<Escape> {
    public override PacketId PacketId => PacketId.Escape;

    public override void Reset() {
    }

    public override void Write(NetworkWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}