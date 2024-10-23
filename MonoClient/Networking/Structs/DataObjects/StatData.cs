using MonoClient.Networking.Enums;

namespace MonoClient.Networking.Structs.DataObjects;

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
        }
        else
            switch (Type) {
                case StatsType.MaterialSatchel:
                    DataObject = new SatchelData();
                    DataObject.Read(reader);
                    break;
                case StatsType.ThresholdData:
                    DataObject = new ThresholdData();
                    DataObject.Read(reader);
                    break;
                case StatsType.StackEffects:
                    DataObject = new StackEffectData();
                    DataObject.Read(reader);
                    break;
                case StatsType.AliveMinions:
                    DataObject = new AliveMinionsData();
                    DataObject.Read(reader);
                    break;
                case StatsType.CondDurations:
                    DataObject = new ConditionDurationData();
                    DataObject.Read(reader);
                    break;
                case StatsType.DungeonModifiers:
                    DataObject = new DungeonModifierData();
                    DataObject.Read(reader);
                    break;
                default:
                    Value = reader.ReadInt32();
                    break;
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
            case StatsType.ItemData0:
            case StatsType.ItemData1:
            case StatsType.ItemData2:
            case StatsType.ItemData3:
            case StatsType.ItemData4:
            case StatsType.ItemData5:
            case StatsType.ItemData6:
            case StatsType.ItemData7:
            case StatsType.ItemData8:
            case StatsType.ItemData9:
            case StatsType.ItemData10:
            case StatsType.ItemData11:
            case StatsType.ItemData12:
            case StatsType.ItemData13:
            case StatsType.ItemData14:
            case StatsType.ItemData15:
            case StatsType.ItemData16:
            case StatsType.ItemData17:
            case StatsType.ItemData18:
            case StatsType.ItemData19:
            case StatsType.ParticleGenerator:
            case StatsType.FlaskItemData0:
            case StatsType.FlaskItemData1:
            case StatsType.FlaskItemData2:
                return true;
            default:
                return false;
        }
    }

    public override string ToString() {
        return $"Type: {Type}, Value: {Value}, Text: {Text}, DataObject: {DataObject}";
    }
}