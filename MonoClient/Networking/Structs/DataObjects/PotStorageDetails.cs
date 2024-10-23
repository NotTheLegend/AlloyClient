namespace MonoClient.Networking.Structs.DataObjects;

public struct PotStorageDetails : IDataObject {
    public int PotionShards;
    public PotStorageDetailType DetailsType;
    public string DetailsJSON;


    public void Reset() {
        PotionShards = 0;
        DetailsType = default;
        DetailsJSON = null;
    }

    // Prolly should actually use these when the client becomes official
    public void Read(NetworkReader reader) {
        PotionShards = reader.ReadInt32();
        DetailsType = (PotStorageDetailType)reader.ReadInt32();
        DetailsJSON = reader.ReadUtf();
    }

    public void Write(NetworkWriter writer) {
        writer.Write(PotionShards);
        writer.Write((int)DetailsType);
        writer.Write(DetailsJSON);
    }

    public override string ToString() {
        return $"PotionShards: {PotionShards}, DetailsType: {DetailsType}, DetailsJSON: {DetailsJSON}";
    }
}