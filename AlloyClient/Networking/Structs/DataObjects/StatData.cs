using AlloyClient.Networking.Enums;

namespace AlloyClient.Networking.Structs.DataObjects;

public struct StatData : IDataObject {
    public StatsType Type;
    public int Value;
    public float FloatValue;
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
            Text = reader.ReadUTF();
        } else if (IsFloatStat(Type)) {
            FloatValue = reader.ReadSingle();
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

    public static bool IsStringStat(StatsType type)
    {
        switch (type)
        {
            case StatsType.Name:
            case StatsType.Guild:
            case StatsType.InventoryData0:
            case StatsType.InventoryData1:
            case StatsType.InventoryData2:
            case StatsType.InventoryData3:
            case StatsType.InventoryData4:
            case StatsType.InventoryData5:
            case StatsType.InventoryData6:
            case StatsType.InventoryData7:
            case StatsType.InventoryData8:
            case StatsType.InventoryData9:
            case StatsType.InventoryData10:
            case StatsType.InventoryData11:
            case StatsType.InventoryData12:
            case StatsType.InventoryData13:
            case StatsType.InventoryData14:
            case StatsType.InventoryData15:
            case StatsType.InventoryData16:
            case StatsType.InventoryData17:
            case StatsType.InventoryData18:
            case StatsType.InventoryData19:
            case StatsType.AbilityDataA:
            case StatsType.AbilityDataB:
            case StatsType.AbilityDataC:
            case StatsType.AbilityDataD:
                return true;
            default:
                return false;
        }
    }
    
    private static bool IsFloatStat(StatsType type)
    {
        return type switch
        {
            // StatsType.CriticalChance => true,
            // StatsType.DodgeChance => true,
            // StatsType.CriticalChanceBonus => true,
            // StatsType.DodgeChanceBonus => true,
            // StatsType.AttackSpeed => true,
            // StatsType.AttackSpeedBonus => true,
            // StatsType.Speed => true,
            // StatsType.SpeedBonus => true,
            _ => false
        };
    }

    public override string ToString() {
        return $"Type: {Type}, Value: {Value}, Text: {Text}, DataObject: {DataObject}";
    }
}