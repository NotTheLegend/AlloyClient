using AlloyClient.Networking.Packets.Outgoing;

namespace AlloyClient.Networking.Packets.Incoming;

public class Ping : IncomingPacket<Ping> {
    public int RTT;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        RTT = 0;
    }

    public override void Read(ref SpanReader reader) {
        RTT = reader.ReadInt32();
    }

    public override void Handle() {
        Client.QueuePacket(Pong.CreatePacket());
    }

    public override string ToString() {
        return $"RTT: {RTT}";
    }
}