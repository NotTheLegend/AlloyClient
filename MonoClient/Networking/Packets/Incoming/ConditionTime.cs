namespace MonoClient.Networking.Packets.Incoming;

public class ConditionTime : IncomingPacket<ConditionTime> {
    public int ObjectId;
    public int InflictorObjectId;
    public byte Effect;
    public int DurationMS;

    public override PacketId PacketId => PacketId.ConditionTime;

    public override void Reset() {
        ObjectId = 0;
        InflictorObjectId = 0;
        Effect = 0;
        DurationMS = 0;
    }

    public override void Read(NetworkReader reader) {
        ObjectId = reader.ReadInt32();
        InflictorObjectId = reader.ReadInt32();
        Effect = reader.ReadByte();
        DurationMS = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"ObjectId: {ObjectId}, InflictorObjectId: {InflictorObjectId}, Effect: {Effect}, DurationMS: {DurationMS}";
    }
}