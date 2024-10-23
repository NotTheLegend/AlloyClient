namespace MonoClient.Networking.Packets.Outgoing;

public class Create : OutgoingPacket<Create> {
    public ushort ClassType;
    public ushort SkinType;

    public override PacketId PacketId => PacketId.Create;

    public override void Reset() {
        ClassType = 0;
        SkinType = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(ClassType);
        writer.Write(SkinType);
    }

    public override string ToString() {
        return $"ClassType: {ClassType}, SkinType: {SkinType}";
    }
}