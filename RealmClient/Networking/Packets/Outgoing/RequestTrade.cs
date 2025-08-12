namespace RealmClient.Networking.Packets.Outgoing;

public class RequestTrade : OutgoingPacket<RequestTrade> {
    public string Name;

    public override PacketId PacketId => PacketId.RequestTrade;

    public override void Reset() {
        Name = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUtf(Name);
    }

    public override string ToString() {
        return $"Name: {Name}";
    }
}