namespace MonoClient.Networking.Packets.Outgoing;

public class ChooseBirthSign : OutgoingPacket<ChooseBirthSign> {
    public string BirthSign;

    public override PacketId PacketId => PacketId.ChooseBirthsign;

    public override void Reset() {
        BirthSign = string.Empty;
    }

    public override void Write(NetworkWriter writer) {
        writer.WriteUtf(BirthSign);
    }

    public override string ToString() {
        return $"BirthSign: {BirthSign}";
    }
}