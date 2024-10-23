namespace MonoClient.Networking.Structs.DataObjects;

public struct SatchelData : IDataObject {
    private static readonly byte[] Empty = new byte[2];
    public ushort[] ObjectTypes;
    public short[] Doses;

    public void Reset() {
        ObjectTypes = null;
        Doses = null;
    }

    public void Read(NetworkReader reader) {
        var len = reader.ReadInt16();
        ObjectTypes = new ushort[len];
        Doses = new short[len];

        for (var i = 0; i < len; i++) {
            ObjectTypes[i] = reader.ReadUInt16();
            Doses[i] = reader.ReadInt16();
        }
    }

    public void Write(NetworkWriter writer) {
        if (ObjectTypes == null) {
            writer.Write(Empty);
            return;
        }

        writer.Write((short)ObjectTypes.Length);

        for (var i = 0; i < ObjectTypes.Length; i++) {
            writer.Write(ObjectTypes[i]);
            writer.Write(Doses[i]);
        }
    }

    public override string ToString() {
        return $"ObjectTypes: {ObjectTypes}, Doses: {Doses}";
    }
}