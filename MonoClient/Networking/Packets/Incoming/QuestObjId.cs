using System.Collections.Generic;

namespace MonoClient.Networking.Packets.Incoming;

public class QuestObjId : IncomingPacket<QuestObjId> {
    public List<int> QuestObjectIds = [];
    public int CurrentQuestObjectId;

    public override PacketId PacketId => PacketId.QuestObjId;

    public override void Reset() {
        QuestObjectIds.Clear();
        CurrentQuestObjectId = 0;
    }

    public override void Read(NetworkReader reader) {
        var count = reader.ReadInt32();

        for (var i = 0; i < count; i++) {
            QuestObjectIds.Add(reader.ReadInt32());
        }

        CurrentQuestObjectId = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"QuestObjectIds: {QuestObjectIds}, CurrentQuestObjectId: {CurrentQuestObjectId}";
    }
}