using System;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game;
using AlloyClient.Game.Objects;
using AlloyClient.Game.Objects.ProjectilePaths;
using AlloyClient.Game.Objects.Util;
using AlloyClient.Networking.Structs.DataObjects;
using AlloyClient.Utils;
using Common;

namespace AlloyClient.Networking.Packets.Incoming;

public class EnemyShoot : IncomingPacket<EnemyShoot> {
    public ushort BulletId;
    public int OwnerId;
    public Position StartingPos;
    public float Angle;
    public short Damage;
    public int PropId;
    public ushort ProjType;
    public ProjectilePath Path;
    public float Lifetime;
    public bool MultiHit;
    public bool PassesCover;
    public bool ArmorPiercing;
    public int Size;
    public (ConditionEffectIndex, int)[] Effects;
    public byte NumShots;
    public float AngleInc;

    public override PacketId PacketId => PacketId.EnemyShoot;

    public override void Reset() {
        BulletId = 0;
        OwnerId = 0;
        ProjType = 0;
        Angle = 0;
        Damage = 0;
        StartingPos.Reset();
        NumShots = 0;
        AngleInc = 0;
        PropId = 0;
    }

    public override void Read(NetworkReader reader) {
        BulletId = reader.ReadUInt16();
        OwnerId = reader.ReadInt32();
        StartingPos.Read(reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
        PropId = reader.ReadInt32();
        if (PropId == -1) {
            ProjType = reader.ReadUInt16();
            Path = ProjectilePath.Read(reader);
            Lifetime = reader.ReadSingle();
            MultiHit = reader.ReadBoolean();
            PassesCover = reader.ReadBoolean();
            ArmorPiercing = reader.ReadBoolean();
            Size = reader.ReadInt32();
            var effCount = reader.ReadUInt16();
            Effects = new (ConditionEffectIndex, int)[effCount];
            for (var i = 0; i < effCount; i++) {
                var eff = reader.ReadUInt16();
                Effects[i] = ((ConditionEffectIndex)eff, eff);
            }
        }
        NumShots = reader.ReadByte();
        AngleInc = reader.ReadSingle();
    }

    public override void Handle() {
        if (!Map.Entities.TryGetValue(OwnerId, out var en))
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
        
        ProjectileProperties projDesc = null;
        ObjectProperties objDesc = en.Properties;
        if (PropId != -1) {
            // Trying getting from Projectiles list
            try {
                projDesc = en.Properties.Projectiles[ProjType];
            }
            // Fallback to dictionary if failed
            catch (Exception e) {
                Logger.Error($"Projectile '{ProjType}' out of range on '{en.Properties.DisplayName}'", "EnemyShoot");
                projDesc = en.Properties.ProjectilesDict[ProjType];
            }
        }

        for (var i = 0; i < NumShots; i++) {
            var proj = ObjectPools.Projectiles.Pop();
            proj.Reset((byte)PropId, Damage, Angle + AngleInc * i, en, objDesc, projDesc ?? ProjectileProperties.FromEnemyShoot(this));
            Map.AddProjectile(proj);
        }

        en.SetAttack(OwnerId, Angle + AngleInc * (NumShots - 1) / 2);
    }

    public override string ToString() {
        return $"BulletId: {BulletId}, OwnerId: {OwnerId}, BulletType: {ProjType}, Angle: {Angle}, Damage: {Damage}, StartingPos: {StartingPos}, NumShots: {NumShots}, AngleInc: {AngleInc}";
    }
}