namespace AlloyClient.Networking.Packets.Outgoing;

public class GuildInvite : OutgoingPacket<GuildInvite> {
    public string Name;

    public override PacketId PacketId => PacketId.GuildInvite;

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