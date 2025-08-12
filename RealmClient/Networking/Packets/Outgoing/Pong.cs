namespace RealmClient.Networking.Packets.Outgoing;

public class Pong : OutgoingPacket<Pong> {
    public override PacketId PacketId => PacketId.Pong;

    public int Serial;

    public int Time;

    public override void Reset() {
        Serial = 0;
        Time = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(Serial);
        writer.Write(Time);
    }

    public override string ToString() {
        return "";
    }
}