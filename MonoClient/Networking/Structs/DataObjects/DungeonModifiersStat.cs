namespace MonoClient.Networking.Structs.DataObjects;

public struct DungeonModifiersStat : IDataObject {
    public int[] MajorModifiers;
    public int[] MinorModifiers;

    public void Reset() {
        MajorModifiers = null;
        MinorModifiers = null;
    }

    public void Read(NetworkReader reader) {
        var majorCount = reader.ReadInt32();
        MajorModifiers = new int[majorCount];

        for (var i = 0; i < majorCount; i++) {
            MajorModifiers[i] = reader.ReadByte();
        }

        var minorCount = reader.ReadInt32();
        MinorModifiers = new int[minorCount];

        for (var i = 0; i < minorCount; i++) {
            MinorModifiers[i] = reader.ReadByte();
        }
    }

    public void Write(NetworkWriter writer) {
        writer.Write(MajorModifiers.Length);

        foreach (var majorModifier in MajorModifiers) {
            writer.Write((byte)majorModifier);
        }

        writer.Write(MinorModifiers.Length);

        foreach (var minorModifier in MinorModifiers) {
            writer.Write((byte)minorModifier);
        }
    }
}