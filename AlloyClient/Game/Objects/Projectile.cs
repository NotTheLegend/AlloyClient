using System;
using System.Collections.Generic;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game.Objects.Util;
using AlloyClient.Networking;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.ParticleEffects;
using AlloyClient.Rendering;
using AlloyClient.Rendering.Types;
using AlloyClient.Ui.Character;
using AlloyClient.Utils;
using Common.Structs;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Objects;

public sealed class Projectile {

    private const double HitTestDelayMs = 16;
    
    private readonly float _jitter = Random.Shared.NextSingle() * 0.00002f - 0.00001f;

    private byte _bulletId;

    private int _damage;

    private float _angle;

    private int _ownerId;

    private bool _damagePlayers;

    private ObjectProperties _objDesc;

    private ProjectileProperties _projDesc;

    private double _startTime;

    private float _angleCorrection;

    private Vector2 _startPosition;

    private Vector2 _position;
    
    public float Rotation;

    public int Size;

    private readonly HashSet<int> _hitEntities = [];
    
    public RenderBase RenderBaseType;

    private double _lastHitTest;

    public void Reset(byte id, int dmg, float angle, Entity entity, ObjectProperties objDesc, ProjectileProperties projDesc) {
        _bulletId = id;
        _damage = dmg;
        _angle = angle;
        _ownerId = entity.ObjectId;
        _damagePlayers = entity is not Player;
        _objDesc = objDesc;
        _projDesc = projDesc;

        Size = _projDesc.Size > 0 ? _projDesc.Size : 100;

        _startTime = Main.GameTime.TotalMs;
        _lastHitTest = 0;
        _angleCorrection = _objDesc.AngleCorrection * MathF.PI / 4;
        _position = _startPosition = entity.Position;
        
        _hitEntities.Clear();
        RenderBaseType = new TypeProjectile();
        RenderBaseType.SetSize(Size);
        RenderBaseType.SetTexture(GetTexture());
    }
    
    public AtlasData GetTexture() {
        var textureData = ObjectLibrary.TypeToTextureData[_objDesc.ObjectType];
        return textureData.HasAnimationData ? textureData.AnimatedTextures.FaceRight[0] : textureData.GetTexture();
    }

    public bool Update(double time, double dt, DepthMatrix matrix) {
        var elapsed = (float)(time - _startTime);

        if (elapsed > _projDesc.LifetimeMs) {
            return false;
        }
        
        UpdateVisibility(matrix);
        
        var newPos = PositionAt(elapsed);

        // Use smart projectile rotation if the projectile does not have its own rotation speed or the NoRotation tag
        if (_objDesc.Rotation != 0) {
            Rotation = elapsed / _objDesc.Rotation; 
        } else if (!_projDesc.NoRotation) {
            var direction = newPos - _position;
            var angle = MathF.Atan2(direction.Y, direction.X);
            Rotation = angle + MathHelper.PiOver2 + Camera.CameraAngle + _angleCorrection;
        }

        // Only do HitTest 60/s instead of 5000+/s lol
        var doHitTest = time - _lastHitTest >= HitTestDelayMs;
        
        if (!MoveTo(newPos) || (doHitTest && HitTest(time))) {
            return false;
        }
        
        RenderBaseType.SetPosition(newPos.X, newPos.Y);
        RenderBaseType.SetRotation(Rotation);

        if (_projDesc.ParticleTrail && doHitTest) {
            Map.AddParticleEffect(new SparkEffect(100, _projDesc.ParticleTrailColor, _projDesc.ParticleTrailLifetime, 0.5f, MathUtils.RandomPlusMinus(3), MathUtils.RandomPlusMinus(3), newPos.X, newPos.Y));
            Map.AddParticleEffect(new SparkEffect(100, _projDesc.ParticleTrailColor, _projDesc.ParticleTrailLifetime, 0.5f, MathUtils.RandomPlusMinus(3), MathUtils.RandomPlusMinus(3), newPos.X, newPos.Y));
            Map.AddParticleEffect(new SparkEffect(100, _projDesc.ParticleTrailColor, _projDesc.ParticleTrailLifetime, 0.5f, MathUtils.RandomPlusMinus(3), MathUtils.RandomPlusMinus(3), newPos.X, newPos.Y));
        }
        
        
        return true;
    }
    
    private void UpdateVisibility(DepthMatrix matrix) {
        var dx = _position.X - Camera.Position.X;
        var dy = _position.Y + Camera.Position.Y;
        var distanceSquared = dx * dx + dy * dy;
        const int playerSightRadiusSquared = Map.TileRenderDistance * Map.TileRenderDistance;
        RenderBaseType.SetVisibility(distanceSquared <= playerSightRadiusSquared);

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
            hit.BulletId = _bulletId;
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
        hit1.Time = (int)time;
        hit1.BulletId = _bulletId;
        hit1.TargetId = enemy.ObjectId;
        hit1.Killed = enemy.Hp <= _damage;
        
        Client.QueuePacket(hit1);
        
        if (!_projDesc.MultiHit)
            return true;

        _hitEntities.Add(enemy.ObjectId);
        return false;
    }

    private Vector2 PositionAt(float elapsed) {
        var finalPosition = _startPosition;
        
        // Projectiles cannot have multiple pattern effects at the same time
        // I'll keep it behaving like flash client
        if (_projDesc.Wavy) {
            return ApplyWavyEffect(finalPosition, elapsed);
        }

        if (_projDesc.Parametric) {
            return ApplyParametricEffect(finalPosition, elapsed);
        }

        if (_projDesc.Boomerang) {
            return ApplyBoomerangEffect(finalPosition, elapsed);
        }

        if (_projDesc.Amplitude != 0) {
            return ApplyAmplitude(finalPosition, elapsed);
        }
        
        // Straight projectile
        var distance = elapsed * _projDesc.Speed;
        
        finalPosition.X += distance * MathF.Cos(_angle);
        finalPosition.Y += distance * MathF.Sin(_angle);

        return finalPosition;
    }

    private Vector2 ApplyWavyEffect(Vector2 origin, float elapsed) {
        var distance = elapsed * _projDesc.Speed;
        var phase = _bulletId % 2 * MathF.PI;
        
        var period = 6 * MathF.PI;
        var amplitude = MathF.PI / 64;
        var theta = _angle + amplitude * MathF.Sin(phase + period + elapsed / 1000);

        origin.X += distance * MathF.Cos(theta);
        origin.Y += distance * MathF.Cos(theta);
            
        return origin;
    }

    private Vector2 ApplyParametricEffect(Vector2 origin, float elapsed) {
        var t = elapsed / _projDesc.LifetimeMs * 2 * MathF.PI;

        var x = MathF.Sin(t) * (_bulletId % 2 == 0 ? -1 : 1);
        var y = MathF.Sin(2 * t) * (_bulletId % 4 < 2 ? 1 : -1);

        origin.X += (x * MathF.Cos(_angle) - y * MathF.Sin(_angle)) * _projDesc.Magnitude;
        origin.Y += (x * MathF.Sin(_angle) + y * MathF.Cos(_angle)) * _projDesc.Magnitude;

        return origin;
    }
    
    private Vector2 ApplyBoomerangEffect(Vector2 origin, float elapsed) {
        var distance = elapsed * _projDesc.Speed;
        var phase = _bulletId % 2 * MathF.PI;
        
        var halfwayDistance = _projDesc.LifetimeMs * _projDesc.Speed / 2;
            
        if (distance > halfwayDistance) 
            distance = halfwayDistance - (distance - halfwayDistance);

        origin.X += distance * MathF.Cos(_angle);
        origin.Y += distance * MathF.Sin(_angle);

        if (_projDesc.Amplitude != 0) {
            var deflection = _projDesc.Amplitude * MathF.Sin(phase + elapsed / _projDesc.LifetimeMs * _projDesc.Frequency * 2 * MathF.PI);
            origin.X += deflection * MathF.Cos(_angle + MathF.PI / 2);
            origin.Y += deflection * MathF.Sin(_angle + MathF.PI / 2);
        }
        
        return origin;
    }
    
    private Vector2 ApplyAmplitude(Vector2 origin, float elapsed) {
        var distance = elapsed * _projDesc.Speed;
        
        origin.X += distance * MathF.Cos(_angle);
        origin.Y += distance * MathF.Sin(_angle);
        
        if (_projDesc.Amplitude != 0) {
            var phase = _bulletId % 2 * MathF.PI;
            var deflection = _projDesc.Amplitude * MathF.Sin(phase + elapsed / _projDesc.LifetimeMs * _projDesc.Frequency * 2 * MathF.PI);
            origin.X += deflection * MathF.Cos(_angle + MathF.PI / 2);
            origin.Y += deflection * MathF.Sin(_angle + MathF.PI / 2);
        }

        return origin;
    }
}