namespace MonoClient.Networking.Packets.Outgoing;

public class JoinGuild : OutgoingPacket<JoinGuild> {
    public string GuildName;

    public override PacketId PacketId => PacketId.JoinGuild;

    public override void Reset() {
        GuildName = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUtf(GuildName);
    }

    public override string ToString() {
        return $"GuildName: {GuildName}";
    }
}