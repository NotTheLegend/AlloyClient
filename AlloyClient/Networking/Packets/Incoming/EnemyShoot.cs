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
using OpenTK.Mathematics;

namespace AlloyClient.Networking.Packets.Incoming;

public class EnemyShoot : IncomingPacket<EnemyShoot> {

    private static readonly Logger _log = new(typeof(EnemyShoot));
    
    public ushort FirstBulletId;
    public int OwnerId;
    public byte ProjectileIndex;
    public Position Offset;
    public float Angle;
    public short Damage;
    public byte NumShots;
    public float AngleInc;
    public ProjectilePath Path;

    public override PacketId PacketId => PacketId.EnemyShoot;

    public override void Reset() {
        FirstBulletId = 0;
        OwnerId = 0;
        ProjectileIndex = 0;
        Angle = 0;
        Damage = 0;
        Offset.Reset();
        NumShots = 0;
        AngleInc = 0;
        Path = null;
    }

    public override void Read(ref SpanReader reader) {
        FirstBulletId = reader.ReadUInt16();
        OwnerId = reader.ReadInt32();
        ProjectileIndex = reader.ReadByte();
        Offset.Read(ref reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt16();
        NumShots = reader.ReadByte();
        AngleInc = reader.ReadSingle();
        Path = ProjectilePath.Read(ref reader);
    }

    public override void Handle() {
        if (!Map.Entities.TryGetValue(OwnerId, out var en))
            return;

        var containerDesc = en.Properties;
        if (!containerDesc.Projectiles.TryGetValue(ProjectileIndex, out var projProps)) {
            _log.Error($"Projectile '{ProjectileIndex}' not found for {en.Name}");
            return;
        }

        if (!ObjectLibrary.IdToObjectType.TryGetValue(projProps.ObjectId, out var objType)) {
            _log.Error($"Projectile '{projProps.ObjectId}' not found in GameData.");
            return;
        }
        
        var objProps = ObjectLibrary.TypeToObjectProps[objType];
        for (var i = 0; i < NumShots; i++) {
            var proj = ObjectPools.Projectiles.Pop();
            proj.Reset((ushort)(FirstBulletId + i), Damage, Angle + AngleInc * i, en, objProps, projProps, Path.Clone(), new Vector2(Offset.X, Offset.Y));
            Map.AddProjectile(proj);
        }

        en.SetAttack(OwnerId, Angle + AngleInc * (NumShots - 1) / 2);
    }

    public override string ToString() {
        return $"BulletId: {FirstBulletId}, OwnerId: {OwnerId}, ProjectileIndex: {ProjectileIndex}, Angle: {Angle}, Damage: {Damage}, StartingPos: {Offset}, NumShots: {NumShots}, AngleInc: {AngleInc}";
    }
}