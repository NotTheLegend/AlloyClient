namespace MonoClient.Networking.Packets.Incoming;

public class DashIndicator : IncomingPacket<DashIndicator> {
    public int ObjectId;
    public float Length;
    public int Duration;
    public float Angle;
    public int SpriteIndex = 1;
    public uint Color = 0xFF0000;

    public override PacketId PacketId => PacketId.DashIndicator;

    public override void Reset() {
        ObjectId = 0;
        Length = 0;
        Duration = 0;
        Angle = 0;
        SpriteIndex = 1;
        Color = 0xFF0000;
    }

    public override void Read(NetworkReader reader) {
        ObjectId = reader.ReadInt32();
        Length = reader.ReadSingle();
        Duration = reader.ReadInt32();
        Angle = reader.ReadSingle();
        SpriteIndex = reader.ReadInt32();
        Color = reader.ReadUInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"ObjectId: {ObjectId}, Length: {Length}, Duration: {Duration}, Angle: {Angle}, SpriteIndex: {SpriteIndex}, Color: {Color}";
    }
}