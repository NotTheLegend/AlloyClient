using RealmClient.Screens.Game.Components.Hud.Chat;
using RealmClient.Ui.Chat;

namespace RealmClient.Networking.Packets.Incoming;

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
        Name = reader.ReadUtf();
        ObjectId = reader.ReadInt32();
        NumStars = reader.ReadInt32();
        BubbleTime = reader.ReadByte();
        Recipient = reader.ReadUtf();
        Txt = reader.ReadUtf();
    }

    public override void Handle() {
        ChatView.QueueChatLine(new ChatLineData {
            Name = Name,
            ObjectId = ObjectId,
            NumStars = NumStars,
            Recipient = Recipient,
            Txt = Txt
        });

        if (Map.Entities.TryGetValue(ObjectId, out var en)) {
            ChatLayer.QueueSpeech(new SpeechData(en, Txt, Recipient));
        }
    }

    public override string ToString() {
        return $"Name: {Name}, ObjectId: {ObjectId}, NumStars: {NumStars}, BubbleTime: {BubbleTime}, Recipient: {Recipient}, Txt: {Txt}";
    }
}