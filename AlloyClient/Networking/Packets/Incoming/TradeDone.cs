namespace AlloyClient.Networking.Packets.Incoming;

public class TradeDone : IncomingPacket<TradeDone> {
    public int Code;
    public string Description;

    public override PacketId PacketId => PacketId.TradeDone;

    public override void Reset() {
        Code = 0;
        Description = null;
    }

    public override void Read(NetworkReader reader) {
        Code = reader.ReadInt32();
        Description = reader.ReadUTF();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"Code: {Code}, Description: {Description}";
    }
}