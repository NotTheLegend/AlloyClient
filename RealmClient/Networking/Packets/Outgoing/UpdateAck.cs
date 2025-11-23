namespace RealmClient.Networking.Packets.Outgoing;

public class UpdateAck : OutgoingPacket<UpdateAck> {
    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
    }

    public override void Write(NetworkWriter writer) {
    }

    public override string ToString() {
        return "";
    }
}