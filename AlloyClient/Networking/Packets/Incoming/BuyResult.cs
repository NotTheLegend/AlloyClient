namespace AlloyClient.Networking.Packets.Incoming;

public class BuyResult : IncomingPacket<BuyResult> {
    public int Result;
    public string ResultString;

    public override PacketId PacketId => PacketId.BuyResult;

    public override void Reset() {
        Result = 0;
        ResultString = null;
    }

    public override void Read(ref SpanReader reader) {
        Result = reader.ReadInt32();
        ResultString = reader.ReadUTF();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Result: {Result}, ResultString: {ResultString}";
    }
}