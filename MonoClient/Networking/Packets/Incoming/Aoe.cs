using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects.Util;

namespace MonoClient.Networking.Packets.Incoming;

public class Aoe : IncomingPacket<Aoe> {
    public Position Pos;
    public float Radius;
    public ushort Damage;
    public ConditionEffectIndex Effect;
    public float Duration;
    public ushort OrigType;

    public override PacketId PacketId => PacketId.Aoe;

    public override void Reset() {
        Pos.Reset();
        Radius = 0;
        Damage = 0;
        Effect = 0;
        Duration = 0;
        OrigType= 0;
    }

    public override void Read(NetworkReader reader) {
        Pos.Read(reader);
        Radius = reader.ReadSingle();
        Damage = reader.ReadUInt16();
        Effect = (ConditionEffectIndex)reader.ReadByte();
        Duration = reader.ReadSingle();
        OrigType = reader.ReadUInt16();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"Pos: {Pos}, Radius: {Radius}, Damage: {Damage}, Effect: {Effect}, Duration: {Duration}, OwnerId: {OrigType}";
    }
}