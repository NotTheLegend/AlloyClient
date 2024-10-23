using System.Numerics;

namespace MonoClient.Networking.Packets.Incoming;

public class Damage : IncomingPacket<Damage> {
    public int TargetId;
    public BigInteger Effects;
    public ushort DamageAmount;
    public bool Kill;
    public byte BulletId;
    public int ObjectId;
    public int EffectId;

    public override PacketId PacketId => PacketId.Damage;


    public override void Reset() {
        TargetId = 0;
        Effects = 0;
        DamageAmount = 0;
        Kill = false;
        BulletId = 0;
        ObjectId = 0;
        EffectId = 0;
    }

    public override void Read(NetworkReader reader) {
        TargetId = reader.ReadInt32();

        var eff = reader.ReadByte();
        for (byte i = 0; i < eff; i++)
            Effects |= (BigInteger)1 << reader.ReadByte();

        DamageAmount = reader.ReadUInt16();
        Kill = reader.ReadBoolean();
        BulletId = reader.ReadByte();
        ObjectId = reader.ReadInt32();
        EffectId = reader.ReadInt32();
    }

    public override void Handle() {
    }

    public override string ToString() {
        return
            $"TargetId: {TargetId}, Effects: {Effects}, DamageAmount: {DamageAmount}, Kill: {Kill}, BulletId: {BulletId}, ObjectId: {ObjectId}, EffectId: {EffectId}";
    }
}