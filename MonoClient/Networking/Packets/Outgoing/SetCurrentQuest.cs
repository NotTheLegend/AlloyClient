namespace MonoClient.Networking.Packets.Outgoing;

public class SetCurrentQuest : OutgoingPacket<SetCurrentQuest> {
    public int QuestObjectId;

    public override PacketId PacketId => PacketId.SetCurrentQuest;

    public override void Reset() {
        QuestObjectId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(QuestObjectId);
    }

    public override string ToString() {
        return $"QuestObjectId: {QuestObjectId}";
    }
}