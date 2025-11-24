namespace AlloyClient.Networking.Packets.Outgoing;

public class CreateGuild : OutgoingPacket<CreateGuild> {
    public string Name;

    public override PacketId PacketId => PacketId.CreateGuild;

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