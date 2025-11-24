using AlloyClient.Networking.Structs.DataObjects;

namespace AlloyClient.Networking.Packets.Outgoing;

public class Move : OutgoingPacket<Move> {

    public int TickId;
    public int Time;
    public Position NewPosition;
    public TimedPosition[] Records;

    public override PacketId PacketId => PacketId.Move;

    public override void Reset() {
        TickId = 0;
        Time = 0;
        NewPosition.Reset();
        Records = [];
    }

    public override void Write(NetworkWriter writer) {
        writer.Write(TickId);
        writer.Write(Time);
        NewPosition.Write(writer);
       
        writer.Write((short)Records.Length);
        for (var i = 0; i < Records.Length; i++) {
            Records[i].Write(writer);
        }
    }

    public override string ToString() {
        return $"NewPosition: {NewPosition}, Time: {Time}";
    }
}