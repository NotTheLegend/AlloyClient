namespace AlloyClient.Networking.Packets.Outgoing;

public class GuildRemove : OutgoingPacket<GuildRemove> {
    public string Name;

    public override PacketId PacketId => PacketId.GuildRemove;

    public override void Reset() {
        Name = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUTF(Name);
    }

    public override string ToString() {
        return $"Name: {Name}";
    }
}