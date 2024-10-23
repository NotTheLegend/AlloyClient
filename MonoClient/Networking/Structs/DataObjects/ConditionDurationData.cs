using System.Collections.Generic;

namespace MonoClient.Networking.Structs.DataObjects;

public struct ConditionDurationData : IDataObject {
    public Dictionary<byte, int> Durations;

    public void Reset() {
        Durations = null;
    }

    public void Read(NetworkReader reader) {
        Durations = new Dictionary<byte, int>();

        var count = reader.ReadByte();

        for (var i = 0; i < count; i++) {
            var id = reader.ReadByte();
            var duration = reader.ReadInt32();
            Durations.Add(id, duration);
        }
    }

    public void Write(NetworkWriter writer) {
        writer.Write((byte)Durations.Count);

        foreach (var pair in Durations) {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }
    }

    public override string ToString() {
        return $"Durations: {Durations.Count}";
    }
}