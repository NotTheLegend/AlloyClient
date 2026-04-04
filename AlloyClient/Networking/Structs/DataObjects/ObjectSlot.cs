namespace AlloyClient.Networking.Structs.DataObjects;

public struct ObjectSlot : IDataObject {
    public int ObjectId;
    public byte SlotId;
    public ushort ObjectType;

    public void Reset() {
        ObjectId = 0;
        SlotId = 0;
        ObjectType = 0;
    }

    public void Read(ref SpanReader reader) {
        ObjectId = reader.ReadInt32();
        SlotId = reader.ReadByte();
        ObjectType = reader.ReadUInt16();
    }

    public void Write(ref SpanWriter writer) {
        writer.Write(ObjectId);
        writer.Write(SlotId);
        writer.Write(ObjectType);
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, SlotId: {SlotId}, ObjectType: {ObjectType}";
    }
}