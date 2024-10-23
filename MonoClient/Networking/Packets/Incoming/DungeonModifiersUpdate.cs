using System.Collections.Generic;
using MonoClient.Networking.Structs.DataObjects;
using static MonoClient.DungeonModifier.DungeonModifiersUtil;

namespace MonoClient.Networking.Packets.Incoming;

public class DungeonModifiersUpdate : IncomingPacket<DungeonModifiersUpdate> {
    public List<int> MajorModifiers;
    public List<int> MinorModifiers;
    public Dictionary<int, DungeonModifierData> ModifiersData;

    public override PacketId PacketId => PacketId.DungeonModifiersUpdate;

    public override void Reset() {
        MajorModifiers = [];
        MinorModifiers = [];
        ModifiersData = new Dictionary<int, DungeonModifierData>();
    }

    public override void Read(NetworkReader reader) {
        var majCount = reader.ReadInt32();

        for (var i = 0; i < majCount; i++) {
            var id = reader.ReadByte();
            MajorModifiers[i] = id;

            var data = new DungeonModifierData((DungeonModifierType)id);
            data.Read(reader);

            ModifiersData[id] = data;
        }

        var minCount = reader.ReadInt32();

        for (var i = 0; i < minCount; i++) {
            var id = reader.ReadByte();
            MinorModifiers[i] = id;

            var data = new DungeonModifierData((DungeonModifierType)id);
            data.Read(reader);

            ModifiersData[id] = data;
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"MajorModifiers: {MajorModifiers}, MinorModifiers: {MinorModifiers}, ModifiersData: {ModifiersData}";
    }
}