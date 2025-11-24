using AlloyClient.Game;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Incoming;

public class Goto : IncomingPacket<Goto> {
    public int ObjectId;
    public Position Pos;

    public override PacketId PacketId => PacketId.Goto;

    public override void Reset() {
        ObjectId = 0;
        Pos.Reset();
    }

    public override void Read(NetworkReader reader) {
        ObjectId = reader.ReadInt32();
        Pos.Read(reader);
    }

    public override void Handle() {
        if (ObjectId == Map.LocalPlayer.ObjectId) {
            Map.LocalPlayer.MoveTo(Pos.X, Pos.Y);
            Map.LocalPlayer.TickPosition.X = Pos.X;
            Map.LocalPlayer.TickPosition.Y = Pos.Y;
            Map.LocalPlayer.PositionAtTick.X = Pos.X;
            Map.LocalPlayer.PositionAtTick.Y = Pos.Y;

            var gt = GotoAck.CreatePacket();
            Client.QueuePacket(gt);
        }
    }

    public override string ToString() {
        return $"ObjectId: {ObjectId}, Pos: {Pos}";
    }
}