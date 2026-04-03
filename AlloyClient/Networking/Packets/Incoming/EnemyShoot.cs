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

    private static readonly Logger _log = new(typeof(EnemyShoot));
    
    public ushort FirstBulletId;
    public int OwnerId;
    public byte ProjectileIndex;
    public Position StartingPos;
    public float Angle;
    public short Damage;
    public byte NumShots;
    public float AngleInc;

    public override PacketId PacketId => PacketId.EnemyShoot;

    public override void Reset() {
        FirstBulletId = 0;
        OwnerId = 0;
        ProjectileIndex = 0;
        Angle = 0;
        Damage = 0;
        StartingPos.Reset();
        NumShots = 0;
        AngleInc = 0;
    }

    public override void Read(NetworkReader reader) {
        FirstBulletId = reader.ReadUInt16();
        OwnerId = reader.ReadInt32();
        ProjectileIndex = reader.ReadByte();
        StartingPos.Read(reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
        NumShots = reader.ReadByte();
        AngleInc = reader.ReadSingle();
    }

    public override void Handle() {
        if (!Map.Entities.TryGetValue(OwnerId, out var en))
            return;

        ObjectProperties objDesc = en.Properties;
        if (!objDesc.Projectiles.TryGetValue(ProjectileIndex, out var projDesc)) {
            _log.Error($"Projectile '{ProjectileIndex}' not found for {en.Name}");
            return;
        }

        for (var i = 0; i < NumShots; i++) {
            var proj = ObjectPools.Projectiles.Pop();
            proj.Reset((byte)(FirstBulletId + i), Damage, Angle + AngleInc * i, en, objDesc, projDesc);
            Map.AddProjectile(proj);
        }

        en.SetAttack(OwnerId, Angle + AngleInc * (NumShots - 1) / 2);
    }

    public override string ToString() {
        return $"BulletId: {FirstBulletId}, OwnerId: {OwnerId}, ProjectileIndex: {ProjectileIndex}, Angle: {Angle}, Damage: {Damage}, StartingPos: {StartingPos}, NumShots: {NumShots}, AngleInc: {AngleInc}";
    }
}