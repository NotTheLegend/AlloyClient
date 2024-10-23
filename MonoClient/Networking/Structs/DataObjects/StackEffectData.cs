using System.Collections.Generic;

namespace MonoClient.Networking.Structs.DataObjects;

public struct StackEffectData : IDataObject {
    public Dictionary<byte, byte> StackEffects;

    public void Reset() {
        StackEffects = new Dictionary<byte, byte>();
    }

    public void Read(NetworkReader reader) {
        StackEffects = new Dictionary<byte, byte>();

        var count = reader.ReadByte();

        for (var i = 0; i < count; i++) {
            var id = reader.ReadByte();
            var stacks = reader.ReadByte();
            StackEffects[id] = stacks;
        }
    }

    public void Write(NetworkWriter writer) {
        writer.Write((byte)StackEffects.Count);

        foreach (var (id, stacks) in StackEffects) {
            writer.Write(id);
            writer.Write(stacks);
        }
    }

    public override string ToString() {
        return $"StackEffects: {StackEffects}";
    }
}