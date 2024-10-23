namespace MonoClient.Networking.Structs.DataObjects;

public struct MarketItem : IDataObject {
    public int Id;
    public int ObjType;
    public int Price;
    public int InsertDate;
    public string Seller;
    public bool Included;
    public string ItemData;
    public int AveragePrice;

    public void Reset() {
        Id = 0;
        ObjType = 0;
        Price = 0;
        InsertDate = 0;
        Seller = string.Empty;
        Included = false;
        ItemData = string.Empty;
        AveragePrice = 0;
    }

    public void Read(NetworkReader reader) {
        Id = reader.ReadInt32();
        ObjType = reader.ReadInt32();
        Price = reader.ReadInt32();
        InsertDate = reader.ReadInt32();
        Seller = reader.ReadUtf();
        Included = reader.ReadBoolean();
        ItemData = reader.ReadUtf();
        AveragePrice = reader.ReadInt32();
    }

    public void Write(NetworkWriter writer) {
        writer.Write(Id);
        writer.Write(ObjType);
        writer.Write(Price);
        writer.Write(InsertDate);
        writer.Write(Seller);
        writer.Write(Included);
        writer.Write(ItemData);
        writer.Write(AveragePrice);
    }

    public override string ToString() {
        return
            $"Id: {Id}, ObjType: {ObjType}, Price: {Price}, InsertDate: {InsertDate}, Seller: {Seller}, Included: {Included}, ItemData: {ItemData}, AveragePrice: {AveragePrice}";
    }
}