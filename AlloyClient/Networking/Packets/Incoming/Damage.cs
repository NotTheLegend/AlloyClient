using System.Numerics;
using AlloyClient.Game.Objects.Util;

namespace AlloyClient.Networking.Packets.Incoming;

public class Damage : IncomingPacket<Damage> {
    public int TargetId;
    public ConditionEffects Effects;
    public ushort DamageAmount;
    public bool Kill;
    public byte BulletId;
    public int ObjectId;

    public override PacketId PacketId => PacketId.Damage;


    public override void Reset() {
        TargetId = 0;
        Effects = 0;
        DamageAmount = 0;
        Kill = false;
        BulletId = 0;
        ObjectId = 0;
    }

    public override void Read(ref SpanReader reader) {
        TargetId = reader.ReadInt32();

        byte c = reader.ReadByte();
        Effects = 0;
        for (int i = 0; i < c; i++)
            Effects |= (ConditionEffects)(1 << reader.ReadByte());

        DamageAmount = reader.ReadUInt16();
        Kill = reader.ReadBoolean();
        BulletId = reader.ReadByte();
        ObjectId = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return $"TargetId: {TargetId}, Effects: {Effects}, DamageAmount: {DamageAmount}, Kill: {Kill}, BulletId: {BulletId}, ObjectId: {ObjectId}";
    }
}