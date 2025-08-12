namespace RealmClient.Networking.Structs.DataObjects;

public struct TradeItem : IDataObject {
    public int Item;
    public int SlotType;
    public bool Tradeable;
    public bool Included;
    public string ItemData;

    public void Reset() {
        Item = 0;
        SlotType = 0;
        Tradeable = false;
        Included = false;
        ItemData = null;
    }

    public void Read(NetworkReader reader) {
        Item = reader.ReadInt32();
        SlotType = reader.ReadInt32();
        Tradeable = reader.ReadBoolean();
        Included = reader.ReadBoolean();
        ItemData = reader.ReadUtf();
    }

    public void Write(NetworkWriter writer) {
        writer.Write(Item);
        writer.Write(SlotType);
        writer.Write(Tradeable);
        writer.Write(Included);
        writer.Write(ItemData);
    }

    public override string ToString() {
        return
            $"Item: {Item}, SlotType: {SlotType}, Tradeable: {Tradeable}, Included: {Included}, ItemData: {ItemData}";
    }
}