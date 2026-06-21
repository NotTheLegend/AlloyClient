using System;
using System.Collections.Generic;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game.Objects.ProjectilePaths;
using AlloyClient.Game.Objects.Util;
using AlloyClient.Networking;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.ParticleEffects;
using AlloyClient.Rendering;
using AlloyClient.Rendering.Types;
using AlloyClient.Ui.Character;
using AlloyClient.Utils;
using Alloy.Common.Structs;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Objects;

public sealed class Projectile : IResettable {

    private const double HitTestDelayMs = 16;
    
    private readonly float _jitter = Random.Shared.NextSingle() * 0.00002f - 0.00001f;

    public RenderBase RenderBaseType;
    public float Rotation;
    public int Size;
    
    public ushort BulletId;
    private int _damage;
    public float Angle;
    private int _ownerId;
    private bool _damagePlayers;
    private ObjectProperties _objDesc;
    private ProjectileProperties _projDesc;
    private double _startTime;
    private float _angleCorrection;
    private Vector2 _startPosition;
    private Vector2 _position;
    private double _lastHitTest;
    public ProjectilePath Path;
    
    private readonly HashSet<int> _hitEntities = [];

    public void Reset(ushort id, int dmg, float angle, Entity entity, ObjectProperties objDesc, ProjectileProperties projDesc, ProjectilePath path, Vector2 startPos) {
        BulletId = id;
        _damage = dmg;
        Angle = angle;
        _ownerId = entity.ObjectId;
        _damagePlayers = entity is not Player;
        _objDesc = objDesc;
        _projDesc = projDesc;
        Path = path;
        Path.SetInfo(new ProjectileInfo() { LifetimeMs = Path.LifetimeMs, ProjId = id, ShootAngle = angle * MathHelper.DegToRad, StartPos = startPos});

        Size = _projDesc.Size > 0 ? _projDesc.Size : 100;

        _startTime = Main.GameTime.TotalMs;
        _lastHitTest = 0;
        _angleCorrection = _objDesc.AngleCorrection * MathF.PI / 4;
        _position = _startPosition = startPos;
        
        _hitEntities.Clear();
        RenderBaseType = new TypeProjectile();
        RenderBaseType.SetSize(Size);
        RenderBaseType.SetTexture(GetTexture());
    }

    public bool IsInPool { get; set; }

    public void Reset() {
        BulletId = 0;
        _damage = 0;
        Angle = 0f;
        _ownerId = 0;
        _damagePlayers = false;
        _objDesc = null;
        _projDesc = null;
        _startTime = 0;
        _lastHitTest = 0;
        _angleCorrection = 0f;
        _startPosition = Vector2.Zero;
        _position = Vector2.Zero;
        _hitEntities.Clear();
        Rotation = 0f;
        Size = 0;
        RenderBaseType = null;
    }

    public AtlasData GetTexture() {
        var textureData = ObjectLibrary.TypeToTextureData[_objDesc.ObjectType];
        return textureData.HasAnimationData ? textureData.AnimatedTextures.FaceRight[0] : textureData.GetTexture();
    }

    public bool Update(double time, double dt, DepthMatrix matrix) {
        var elapsed = (float)(time - _startTime);

        if (elapsed > Path.LifetimeMs) {
            return false;
        }
        
        UpdateVisibility(matrix);

        var deltaPos = PositionAt(elapsed);
        var newPos = _startPosition + deltaPos;
        // Console.WriteLine($"Proj pos: {newPos} ({_startPosition}|{deltaPos})");

        // Use smart projectile rotation if the projectile does not have its own rotation speed or the NoRotation tag
        if (_objDesc.Rotation != 0) {
            Rotation = elapsed / _objDesc.Rotation;
        } else if (!_projDesc.NoRotation) {
            var direction = newPos - _position;
            var angle = MathF.Atan2(direction.Y, direction.X);
            Rotation = angle + Settings.CameraAngle + _angleCorrection;
        }

        // Only do HitTest 60/s instead of 5000+/s lol
        var doHitTest = time - _lastHitTest >= HitTestDelayMs;
        
        if (!MoveTo(newPos) || (doHitTest && HitTest(time))) {
            return false;
        }
        
        RenderBaseType.SetPosition(newPos.X, newPos.Y);
        RenderBaseType.SetRotation(Rotation);

        if (_projDesc.ParticleTrail && doHitTest) {
            Map.AddParticleEffect(new SparkEffect(100, _projDesc.ParticleTrailColor, _projDesc.ParticleTrailLifetime, 0.5f, Random.Shared.PlusMinus(3f), Random.Shared.PlusMinus(3f), newPos.X, newPos.Y));
            Map.AddParticleEffect(new SparkEffect(100, _projDesc.ParticleTrailColor, _projDesc.ParticleTrailLifetime, 0.5f, Random.Shared.PlusMinus(3f), Random.Shared.PlusMinus(3f), newPos.X, newPos.Y));
            Map.AddParticleEffect(new SparkEffect(100, _projDesc.ParticleTrailColor, _projDesc.ParticleTrailLifetime, 0.5f, Random.Shared.PlusMinus(3f), Random.Shared.PlusMinus(3f), newPos.X, newPos.Y));
        }
        
        
        return true;
    }
    
    private void UpdateVisibility(DepthMatrix matrix) {
        /*var dx = _position.X - Camera.Position.X;
        var dy = _position.Y + Camera.Position.Y;
        var distanceSquared = dx * dx + dy * dy;
        const int playerSightRadiusSquared = Map.TileRenderDistance * Map.TileRenderDistance;
        RenderBaseType.SetVisibility(distanceSquared <= playerSightRadiusSquared);*/
        RenderBaseType.SetVisibility(true);

        var sort = _position.X * matrix.M12 + _position.Y * matrix.M22 + matrix.M42;
        RenderBaseType.SetDepth(0.5f + 0.4f * sort + _jitter);
    }
    
    private bool MoveTo(Vector2 pos) {
        var tile = Map.GetTile(pos);

        if (tile == null || tile.Type == 0xFF) {
            //todo hit effect
            return false;
        }
        
        if (tile.OccupiedObject != null) {
            var obj = tile.OccupiedObject.Properties;
            if ((!obj.IsEnemy || _damagePlayers) && (obj.EnemyOccupySquare || !_projDesc.PassesCover && obj.OccupySquare)) {
                //todo hit effect
                return false;
            }
        }

        _position.X = pos.X;
        _position.Y = pos.Y;
        
        RenderBaseType.SetPosition(pos.X, pos.Y);
        
        return true;
    }
    
    private bool HitTest(double time) {
        _lastHitTest = time;
        
        if (_damagePlayers) {

            var target = EntityUtils.GetClosestPlayer(_position, 0.5f);

            if (target == null || _hitEntities.Contains(target.ObjectId))
                return false;
            
            Map.AddParticleEffect(new HitEffect(target, 0xFF0000));
            NotificationLayer.AddStatusText(target, $"-{_damage}", 0xFF0000, 1000, 0);
            
            var hit = PlayerHit.CreatePacket();
            hit.BulletId = BulletId;
            hit.ObjectId = _ownerId;
            
            Client.QueuePacket(hit);

            if (!_projDesc.MultiHit)
                return true;

            _hitEntities.Add(target.ObjectId);
            return false;
        }

        var enemy = EntityUtils.GetClosestEnemy(_position, 0.5f);

        if (enemy == null || _hitEntities.Contains(enemy.ObjectId))
            return false;

        Map.AddParticleEffect(new HitEffect(enemy, 0xFF0000));
        NotificationLayer.AddStatusText(enemy, $"-{_damage}", 0xFF0000, 1000, 0);
        
        var hit1 = EnemyHit.CreatePacket();
        hit1.BulletId = BulletId;
        hit1.TargetId = enemy.ObjectId;
        
        Client.QueuePacket(hit1);
        
        if (!_projDesc.MultiHit)
            return true;

        _hitEntities.Add(enemy.ObjectId);
        return false;
    }

    private Vector2 PositionAt(float elapsed) {
        return Path.PositionAt(elapsed);
    }
}