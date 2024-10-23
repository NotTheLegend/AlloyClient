using System;
using MonoClient.Assets.Libraries;
using MonoClient.Objects;
using MonoClient.Objects.Enums;
using MonoClient.State;
using MonoClient.Utils;

namespace MonoClient.Networking.Packets.Incoming;

public class EnemyShoot : IncomingPacket<EnemyShoot> {
    public byte BulletId;
    public int OwnerId;
    public byte BulletType;
    public float Angle;
    public short Damage;
    public byte NumShots;
    public float AngleInc;
    public int ContainerType;
    public float OffsetX;
    public float OffsetY;
    public byte StartBulletType;
    public float StartAngle;
    public int StartPosTarget;
    public bool ArmorPierce;
    public int CriticalHits;
    public bool TrueDamage;
    public bool ContactDamage;
    public string ProjDesc = "{}";

    public override PacketId PacketId => PacketId.EnemyShoot;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        BulletType = 0;
        Angle = 0;
        Damage = 0;
        ContainerType = 0;
        NumShots = 0;
        AngleInc = 0;
        OffsetX = 0;
        OffsetY = 0;
        StartBulletType = 0;
        StartAngle = 0;
        StartPosTarget = 0;
        ArmorPierce = false;
        CriticalHits = 0;
        TrueDamage = false;
        ContactDamage = false;
        ProjDesc = "{}";
    }

    public override void Read(NetworkReader reader) {
        BulletId = reader.ReadByte();
        OwnerId = reader.ReadInt32();
        BulletType = reader.ReadByte();
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
        ContainerType = reader.ReadInt32();
        NumShots = reader.ReadByte();
        AngleInc = reader.ReadSingle();
        OffsetX = reader.ReadSingle();
        OffsetY = reader.ReadSingle();
        StartBulletType = reader.ReadByte();
        StartAngle = reader.ReadSingle();
        StartPosTarget = reader.ReadInt32();
        ArmorPierce = reader.ReadBoolean();
        CriticalHits = reader.ReadInt32();
        TrueDamage = reader.ReadBoolean();
        ContactDamage = reader.ReadBoolean();
        ProjDesc = reader.ReadUtf();
    }

    public override void Handle() {
        if (!Map.Entities.TryGetValue(OwnerId, out var en))
            return;

        if (en.Properties.ObjectId == "Pirate")
            return;

        var type = ObjectLibrary.IdToObjectType[en.Properties.Projectiles[BulletType].ObjectId];
        for (var i = 0; i < NumShots; i++) {
            var proj = new Projectile {
                Properties = ObjectLibrary.TypeToObjectProps[type],
                ProjDesc = en.Properties.Projectiles[BulletType],
                StartX = en.Position.X,
                StartY = en.Position.Y,
                Angle = Angle + AngleInc * i,
                StartTime = en.Timer
            };
            proj.SetObjectId(BulletId);
            proj.SetType(type);
            proj.SetPos(en.Position.X, en.Position.Y);
            proj.SetRotation();
            
            Map.AddProjectile(proj);
        }

        en.SetAttack(ContainerType, Angle + AngleInc * (NumShots - 1) / 2);
        en.AnimationType = AnimationType.Attack;
        en.IsShooting = true;
    }

    public override string ToString() {
        return
            $"BulletId: {BulletId}, OwnerId: {OwnerId}, BulletType: {BulletType}, Angle: {Angle}, Damage: {Damage}, ContainerType: {ContainerType}, NumShots: {NumShots}, AngleInc: {AngleInc}, OffsetX: {OffsetX}, OffsetY: {OffsetY}, StartBulletType: {StartBulletType}, StartAngle: {StartAngle}, StartPosTarget: {StartPosTarget}, ArmorPierce: {ArmorPierce}, CriticalHits: {CriticalHits}, TrueDamage: {TrueDamage}, ContactDamage: {ContactDamage}, ProjDesc: {ProjDesc}";
    }
}