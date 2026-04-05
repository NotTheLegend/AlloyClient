namespace AlloyClient.Networking.Packets.Incoming;

public class QuestObjId : IncomingPacket<QuestObjId> {
    public int CurrentQuestObjectId;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        CurrentQuestObjectId = 0;
    }

    public override void Read(ref SpanReader reader) {
        CurrentQuestObjectId = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"CurrentQuestObjectId: {CurrentQuestObjectId}";
    }
}