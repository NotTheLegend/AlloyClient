using RealmClient.Networking.Enums;

namespace RealmClient.Networking.Structs.DataObjects;

public struct StatData : IDataObject {
    public StatsType Type;
    public int Value;
    public string Text;

    public IDataObject DataObject;

    public void Reset() {
        Type = default;
        Value = 0;
        Text = null;
        DataObject = null;
    }

    public void Read(NetworkReader reader) {
        Type = (StatsType)reader.ReadByte();

        if (IsStringStat(Type)) {
            Text = reader.ReadUtf();
        } else {
            Value = reader.ReadInt32();
        }
    }

    public void Write(NetworkWriter writer) {
        writer.Write((byte)Type);

        if (IsStringStat(Type)) {
            writer.Write(Text);
        }
        else {
            writer.Write(Value);
        }
    }

    private static bool IsStringStat(StatsType type) {
        switch (type) {
            case StatsType.Name:
            case StatsType.Guild:
                return true;
            default:
                return false;
        }
    }

    public override string ToString() {
        return $"Type: {Type}, Value: {Value}, Text: {Text}, DataObject: {DataObject}";
    }
}