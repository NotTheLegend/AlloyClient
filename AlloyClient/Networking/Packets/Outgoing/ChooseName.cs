namespace AlloyClient.Networking.Packets.Outgoing;

public class ChooseName : OutgoingPacket<ChooseName> {
    public string Name;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        Name = string.Empty;
    }

    public override void Write(ref SpanWriter writer) {
        writer.WriteUTF(Name);
    }

    public override string ToString() {
        return $"Name: {Name}";
    }
}