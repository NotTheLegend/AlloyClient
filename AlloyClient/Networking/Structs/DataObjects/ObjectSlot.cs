namespace AlloyClient.Networking.Structs.DataObjects;

public struct ObjectSlot : IDataObject {
    public int ObjectId;
    public byte SlotId;

    public void Reset() {
        ObjectId = 0;
        SlotId = 0;
    }

    public void Read(ref SpanReader reader) {
        ObjectId = reader.ReadInt32();
        SlotId = reader.ReadByte();
    }

    public void Write(ref SpanWriter writer) {
        writer.Write(ObjectId);
        writer.Write(SlotId);
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, SlotId: {SlotId}";
    }
}