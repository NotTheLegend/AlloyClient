namespace MonoClient.Networking.Packets.Outgoing;

public class ActivateGround : OutgoingPacket<ActivateGround> {
    public int X;
    public int Y;

    public override PacketId PacketId => PacketId.ActivateGround;

    public override void Reset() {
        X = 0;
        Y = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(X);
        writer.Write(Y);
    }

    public override string ToString() {
        return $"X: {X}, Y: {Y}";
    }
}