using System;
using MonoClient.Assets.Libraries;
using MonoClient.Assets.XmlStructs;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects;
using MonoClient.Objects.Enums;
using MonoClient.ParticleEffects;
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

        // BulletType can be out of range in properties projectiles because
        // ObjectProperties Projectiles list does not store the ProjectileProperties by id
        // Ex: (Septavius the Ghost God)
        // Enemy has 5 projectiles, with ids 0, 1, 2, 4 and 5, missing id 3
        // Projectiles List in ObjectProperties will store 5 ProjectileProperties
        // Enemy shoots projectile with id '5'
        // Projectiles[5] is out of range
        // --------------------------------------------------------------------------------------------
        // I'm just adding a dictionary in ObjectProperties for storing projectiles
        // as a temporary fix until we decide to use dictionary or do something else
        // because I'm not sure what other part of the client uses Projectiles list and in what way
        
        ushort type;
        ProjectileProperties projDesc;
        // Trying getting from Projectiles list
        try {
            type = ObjectLibrary.IdToObjectType[en.Properties.Projectiles[BulletType].ObjectId];
            projDesc = en.Properties.Projectiles[BulletType];
        } 
        // Fallback to dictionary if failed
        catch (Exception e) {
            Logger.Error($"Projectile '{BulletType}' out of range on '{en.Properties.DisplayName}'", "EnemyShoot");
            type = ObjectLibrary.IdToObjectType[en.Properties.ProjectilesDict[BulletType].ObjectId];
            projDesc = en.Properties.ProjectilesDict[BulletType];
        }
        
        for (var i = 0; i < NumShots; i++) {
            var proj = new Projectile {
                Properties = ObjectLibrary.TypeToObjectProps[type],
                ProjDesc = projDesc,
                Angle = Angle + AngleInc * i,
                StartPosition = en.Position,
                StartTime = en.Timer
            };
            proj.Owner = en;
            
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