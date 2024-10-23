namespace MonoClient.Networking.Packets.Incoming;

public class ReforgeItemResult : IncomingPacket<ReforgeItemResult> {
    public string ResultReforge;
    public string ErrorText;
    public int SlotId;
    public bool ReforgeStone;

    public override PacketId PacketId => PacketId.ReforgeItemResult;

    public override void Reset() {
        ResultReforge = null;
        ErrorText = null;
        SlotId = 0;
        ReforgeStone = false;
    }

    public override void Read(NetworkReader reader) {
        ResultReforge = reader.ReadUtf();
        ErrorText = reader.ReadUtf();
        SlotId = reader.ReadInt32();
        ReforgeStone = reader.ReadBoolean();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"ResultReforge: {ResultReforge}, ErrorText: {ErrorText}, SlotId: {SlotId}, ReforgeStone: {ReforgeStone}";
    }
}