namespace AlloyClient.Networking.Packets.Incoming;

public class InvitedToGuild : IncomingPacket<InvitedToGuild> {
    public string Name;
    public string GuildName;

    public override PacketId PacketId => PacketId.InvitedToGuild;

    public override void Reset() {
        Name = null;
        GuildName = null;
    }

    public override void Read(ref SpanReader reader) {
        Name = reader.ReadUTF();
        GuildName = reader.ReadUTF();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Name: {Name}, GuildName: {GuildName}";
    }
}