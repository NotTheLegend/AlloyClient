namespace MonoClient.Networking.Structs.DataObjects;

public struct BidHistoryItem : IDataObject {
    public string PlayerName;
    public int BidAmount;

    public void Reset() {
        PlayerName = null;
        BidAmount = 0;
    }

    public void Read(NetworkReader reader) {
        PlayerName = reader.ReadUtf();
        BidAmount = reader.ReadInt32();
    }

    public void Write(NetworkWriter writer) {
        writer.Write(PlayerName);
        writer.Write(BidAmount);
    }

    public override string ToString() {
        return $"PlayerName: {PlayerName}, BidAmount: {BidAmount}";
    }
}