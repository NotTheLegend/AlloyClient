using System;
using MonoClient.Assets.Libraries;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects;
using MonoClient.Objects.Enums;
using MonoClient.State;
using MonoClient.Utils;

namespace MonoClient.Networking.Packets.Incoming;

public class EnemyShoot : IncomingPacket<EnemyShoot> {
    public byte BulletId;
    public int OwnerId;
    public byte BulletType;
    public Position StartingPos;
    public float Angle;
    public short Damage;
    public byte NumShots;
    public float AngleInc;

    public override PacketId PacketId => PacketId.EnemyShoot;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        BulletType = 0;
        Angle = 0;
        Damage = 0;
        StartingPos.Reset();
        NumShots = 0;
        AngleInc = 0;
    }

    public override void Read(NetworkReader reader) {
        BulletId = reader.ReadByte();
        OwnerId = reader.ReadInt32();
        BulletType = reader.ReadByte();
        StartingPos.Read(reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
        NumShots = reader.ReadByte();
        AngleInc = reader.ReadSingle();
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

        en.SetAttack(OwnerId, Angle + AngleInc * (NumShots - 1) / 2);
        en.AnimationType = AnimationType.Attack;
        en.IsShooting = true;
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, OwnerId: {OwnerId}, BulletType: {BulletType}, Angle: {Angle}, Damage: {Damage}, StartingPos: {StartingPos}, NumShots: {NumShots}, AngleInc: {AngleInc}";
    }
}