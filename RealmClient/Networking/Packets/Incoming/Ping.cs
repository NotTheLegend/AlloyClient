using RealmClient.Networking.Packets.Outgoing;

namespace RealmClient.Networking.Packets.Incoming;

public class Ping : IncomingPacket<Ping> {
    public int RTT;

    public override PacketId PacketId => PacketId.Ping;

    public override void Reset() {
        RTT = 0;
    }

    public override void Read(NetworkReader reader) {
        RTT = reader.ReadInt32();
    }

    public override void Handle() {
        Client.QueuePacket(Pong.CreatePacket());
    }

    public override string ToString() {
        return $"RTT: {RTT}";
    }
}