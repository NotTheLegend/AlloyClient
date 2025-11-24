namespace AlloyClient.Networking.Packets.Outgoing;

public class ChangeGuildRank : OutgoingPacket<ChangeGuildRank> {
    public string Name;
    public int GuildRank;

    public override PacketId PacketId => PacketId.ChangeGuildRank;

    public override void Reset() {
        Name = string.Empty;
        GuildRank = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUTF(Name);
        writer.Write(GuildRank);
    }

    public override string ToString() {
        return $"Name: {Name}, GuildRank: {GuildRank}";
    }
}