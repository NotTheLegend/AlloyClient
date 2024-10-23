namespace MonoClient.Networking.Packets.Outgoing;

public class Load : OutgoingPacket<Load> {
    public int CharId;

    public override PacketId PacketId => PacketId.Load;

    public override void Reset() {
        CharId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(CharId);
        writer.WriteUtf("air0");
    }

    public override string ToString() {
        return $"CharId: {CharId}";
    }
}