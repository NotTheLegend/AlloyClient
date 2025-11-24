namespace RealmClient.Networking.Packets.Outgoing;

public class EditAccountList : OutgoingPacket<EditAccountList> {
    public int AccountListId;
    public bool Add;
    public int ObjectId;

    public override PacketId PacketId => PacketId.Unknown;

    public override void Reset() {
        AccountListId = 0;
        Add = false;
        ObjectId = 0;
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(AccountListId);
        writer.Write(Add);
        writer.Write(ObjectId);
    }

    public override string ToString() {
        return $"AccountListId: {AccountListId}, Add: {Add}, ObjectId: {ObjectId}";
    }
}