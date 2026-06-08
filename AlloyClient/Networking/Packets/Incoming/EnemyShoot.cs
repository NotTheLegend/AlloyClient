using AlloyClient.Assets.Libraries;
using AlloyClient.Game;
using AlloyClient.Game.Objects;
using AlloyClient.Game.Objects.ProjectilePaths;
using AlloyClient.Networking.Structs.DataObjects;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace AlloyClient.Networking.Packets.Incoming;

public class EnemyShoot : IncomingPacket<EnemyShoot> {

    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(EnemyShoot));
    
    public ushort FirstBulletId;
    public int OwnerId;
    public byte ProjectileIndex;
    public Position StartPos;
    public float Angle;
    public int Damage;
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
        StartPos.Reset();
        NumShots = 0;
        AngleInc = 0;
        Path = null;
    }

    public override void Read(ref SpanReader reader) {
        FirstBulletId = reader.ReadUInt16();
        OwnerId = reader.ReadInt32();
        ProjectileIndex = reader.ReadByte();
        StartPos.Read(ref reader);
        Angle = reader.ReadSingle();
        Damage = reader.ReadInt32();
        NumShots = reader.ReadByte();
        AngleInc = reader.ReadSingle();
        Path = ProjectilePath.Read(ref reader);
    }

    public override void Handle() {
        if (!Map.Entities.TryGetValue(OwnerId, out var en))
            return;

        var containerDesc = en.Properties;
        if (!containerDesc.Projectiles.TryGetValue(ProjectileIndex, out var projProps)) {
            Logger.Log(LogLevel.Error, $"Projectile '{ProjectileIndex}' not found for {en.Name}");
            return;
        }
        
        if (!ObjectLibrary.IdToObjectType.TryGetValue(projProps.ObjectId, out var objType)) {
            Logger.Log(LogLevel.Error, $"Projectile '{projProps.ObjectId}' not found in GameData.");
            return;
        }
        
        var objProps = ObjectLibrary.TypeToObjectProps[objType];
        for (var i = 0; i < NumShots; i++) {
            var proj = ObjectPools.Projectiles.Pop();
            proj.Reset((ushort)(FirstBulletId + i), Damage, Angle + AngleInc * i, en, objProps, projProps, Path.Clone(), new Vector2(StartPos.X, StartPos.Y));
            Map.AddProjectile(proj);
        }

        en.SetAttack(OwnerId, Angle + AngleInc * (NumShots - 1) / 2);
    }

    public override string ToString() {
        return $"BulletId: {FirstBulletId}, OwnerId: {OwnerId}, ProjectileIndex: {ProjectileIndex}, Angle: {Angle}, Damage: {Damage}, StartingPos: {StartPos}, NumShots: {NumShots}, AngleInc: {AngleInc}";
    }
}