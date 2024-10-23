namespace MonoClient.Networking.Structs.DataObjects;

public struct AliveMinionsData : IDataObject {
    public int[] Minions;

    public void Reset() {
        Minions = null;
    }

    public void Read(NetworkReader reader) {
        var count = reader.ReadByte();
        Minions = new int[count];

        for (var i = 0; i < count; i++) {
            Minions[i] = reader.ReadInt32();
        }
    }

    public void Write(NetworkWriter writer) {
        writer.Write((byte)Minions.Length);

        foreach (var minion in Minions) {
            writer.Write(minion);
        }
    }

    public override string ToString() {
        return $"Minions: {string.Join(", ", Minions)}";
    }
}