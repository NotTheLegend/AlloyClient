namespace MonoClient.Networking.Packets.Incoming;

public class AccountList : IncomingPacket<AccountList> {
    public int AccountListId;
    public string[] AccountIds;

    public override PacketId PacketId => PacketId.AccountList;

    public override void Reset() {
        AccountListId = 0;
        AccountIds = null;
    }

    public override void Read(NetworkReader reader) {
        AccountListId = reader.ReadInt32();

        AccountIds = new string[reader.ReadInt16()];

        for (var i = 0; i < AccountIds.Length; i++) {
            AccountIds[i] = reader.ReadUtf();
        }
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"AccountListId: {AccountListId}, AccountIds: {string.Join(", ", AccountIds)}";
    }
}