using AlloyClient.Game;

namespace AlloyClient.Networking.Packets.Incoming;

public class AccountList : IncomingPacket<AccountList> {
    public int AccountListId;
    public int[] AccountIds;

    public override PacketId PacketId => PacketId.AccountList;

    public override void Reset() {
        AccountListId = 0;
        AccountIds = null;
    }

    public override void Read(NetworkReader reader) {
        AccountListId = reader.ReadInt32();

        AccountIds = new int[reader.ReadInt16()];

        for (var i = 0; i < AccountIds.Length; i++) {
            AccountIds[i] = int.Parse(reader.ReadUTF());
        }
    }

    public override void Handle() {
        PartyData.SetData(AccountListId, AccountIds);
    }

    public override string ToString() {
        return $"AccountListId: {AccountListId}, AccountIds: {string.Join(", ", AccountIds)}";
    }
}