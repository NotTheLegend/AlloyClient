using AlloyClient.Game;
using AlloyClient.Game.Components.Hud.Chat;
using AlloyClient.Ui.Chat;

namespace AlloyClient.Networking.Packets.Incoming;

public class Text : IncomingPacket<Text> {
    public string Name;
    public int ObjectId;
    public int NumStars;
    public byte BubbleTime;
    public string Recipient;
    public string Txt;

    public override PacketId PacketId => PacketId.Text;

    public override void Reset() {
        Name = null;
        ObjectId = 0;
        NumStars = 0;
        BubbleTime = 0;
        Recipient = null;
        Txt = null;
    }

    public override void Read(NetworkReader reader) {
        Name = reader.ReadUTF();
        ObjectId = reader.ReadInt32();
        NumStars = reader.ReadInt32();
        BubbleTime = reader.ReadByte();
        Recipient = reader.ReadUTF();
        Txt = reader.ReadUTF();
    }

    public override void Handle() {
        ChatBox.AddChatLine.Dispatch(new ChatBoxLineData(Main.GetTime(), Name, NumStars, Recipient, Txt));

        if (Map.Entities.TryGetValue(ObjectId, out var en)) {
            ChatLayer.QueueSpeech(new SpeechData(en, Txt, Recipient));
        }
    }

    public override string ToString() {
        return $"Name: {Name}, ObjectId: {ObjectId}, NumStars: {NumStars}, BubbleTime: {BubbleTime}, Recipient: {Recipient}, Txt: {Txt}";
    }
}