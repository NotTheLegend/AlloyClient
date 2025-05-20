using System;
using System.Collections.Generic;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Assets.Libraries;
using MonoClient.Assets.XmlStructs;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Objects.Util;
using MonoClient.ParticleEffects;
using MonoClient.Rendering;
using MonoClient.Rendering.Types;
using MonoClient.State;
using MonoClient.Ui.Character;
using MonoClient.Utils;

namespace MonoClient.Objects;

public sealed class Projectile {

    public static readonly ObjectPool<Projectile> Pool = new();
    
    private readonly float _jitter = Random.Shared.NextSingle() * 0.00002f - 0.00001f;
    
    public bool PendingRemoval;

    public byte BulletId;

    private int _damage;

    private float _angle;

    private int _ownerId;

    private bool _damagePlayers;

    private ObjectProperties _objDesc;

    private ProjectileProperties _projDesc;

    private double _startTime;

    private float _angleCorrection;

    private Vector2 _startPosition;
    
    public Vector2 Position;
    
    public float Rotation;

    public int Size;

    private readonly HashSet<int> _hitEntities = [];

    private ParticleEffect _effect;
    
    public RenderBase RenderBaseType;

    public void Reset(byte id, int dmg, float angle, Entity entity, ObjectProperties objDesc, ProjectileProperties projDesc) {
        BulletId = id;
        _damage = dmg;
        _angle = angle;
        _ownerId = entity.ObjectId;
        _damagePlayers = entity is not Player;
        _objDesc = objDesc;
        _projDesc = projDesc;

        Size = _projDesc.Size > 0 ? _projDesc.Size : 100;

        _startTime = Map.CurrentTime;
        _angleCorrection = _objDesc.AngleCorrection * MathF.PI / 4;
        _startPosition = entity.Position;
        
        _hitEntities.Clear();
        _effect = null;
        RenderBaseType = new TypeProjectile(this);
    }
    
    public AtlasData GetTexture() {
        var textureData = ObjectLibrary.TypeToTextureData[_objDesc.ObjectType];
        return textureData.HasAnimationData ? textureData.AnimatedTextures.FaceRight[0] : textureData.GetTexture();
    }

    public void Update(double time, double dt) {
        var elapsed = (float)time - (float)_startTime;

        if (elapsed > _projDesc.LifetimeMs) {
            SetRemoval();
            return;
        }
        
        var newPos = PositionAt(elapsed);

        // Use smart projectile rotation if the projectile does not have its own rotation speed or the NoRotation tag
        if (_objDesc.Rotation != 0) {
            Rotation = elapsed / _objDesc.Rotation; 
        } else if (!_projDesc.NoRotation) {
            var direction = newPos - Position;
            var angle = MathF.Atan2(direction.Y, direction.X);
            Rotation = angle + Camera.CameraAngle + _angleCorrection;
        }
        
        
        if (!MoveTo(newPos) || HitTest(time)) {
            SetRemoval();
            return;
        }

        _effect?.Update(time, dt);
        RenderBaseType.SetPosition(newPos.X, newPos.Y);
    }
    
    public void UpdateVisibility(ref Matrix matrix) {
        var dx = Position.X - Camera.Position.X;
        var dy = Position.Y + Camera.Position.Y;
        var distanceSquared = dx * dx + dy * dy;
        const int playerSightRadiusSquared = Map.TileRenderDistance * Map.TileRenderDistance;
        RenderBaseType.SetVisibility(distanceSquared <= playerSightRadiusSquared);
        
        var sort = Vector3.Transform(new Vector3(Position.X, Position.Y, 0), matrix).Y;
        RenderBaseType.SetDepth(0.5f + 0.4f * sort + _jitter);
    }

    private void SetRemoval() {
        PendingRemoval = true;
        Pool.Push(this);
        Map.EntityStorage.Remove(this);
    }
    
    private bool MoveTo(Vector2 pos) {
        var tile = Map.GetTile(pos);

        if (tile == null) {
            return false;
        }
        
        if (tile.OccupiedObject != null && tile.OccupiedObject.Properties.OccupySquare) {
            return false;
        }

        Position.X = pos.X;
        Position.Y = pos.Y;
        
        RenderBaseType.SetPosition(pos.X, pos.Y);
        
        return true;
    }
    
    private bool HitTest(double time) {
        if (_damagePlayers) {

            var target = EntityUtils.GetClosestPlayer(Position, 0.5f);

            if (target == null || _hitEntities.Contains(target.ObjectId))
                return false;
            
            target.Effect = new HitEffect(target, 0xff0000);
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

        var enemy = EntityUtils.GetClosestEnemy(Position, 0.5f);

        if (enemy == null || _hitEntities.Contains(enemy.ObjectId))
            return false;

        enemy.Effect = new HitEffect(enemy, 0xFF0000);
        NotificationLayer.AddStatusText(enemy, $"-{_damage}", 0xFF0000, 1000, 0);
        
        var hit1 = EnemyHit.CreatePacket();
        hit1.Time = (int)time;
        hit1.BulletId = BulletId;
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
        var phase = BulletId % 2 * MathF.PI;
        
        var period = 6 * MathF.PI;
        var amplitude = MathF.PI / 64;
        var theta = _angle + amplitude * MathF.Sin(phase + period + elapsed / 1000);

        origin.X += distance * MathF.Cos(theta);
        origin.Y += distance * MathF.Cos(theta);
            
        return origin;
    }

    private Vector2 ApplyParametricEffect(Vector2 origin, float elapsed) {
        var t = elapsed / _projDesc.LifetimeMs * 2 * MathF.PI;

        var x = MathF.Sin(t) * (BulletId % 2 == 0 ? -1 : 1);
        var y = MathF.Sin(2 * t) * (BulletId % 4 < 2 ? 1 : -1);

        origin.X += (x * MathF.Cos(_angle) - y * MathF.Sin(_angle)) * _projDesc.Magnitude;
        origin.Y += (x * MathF.Sin(_angle) + y * MathF.Cos(_angle)) * _projDesc.Magnitude;

        return origin;
    }
    
    private Vector2 ApplyBoomerangEffect(Vector2 origin, float elapsed) {
        var distance = elapsed * _projDesc.Speed;
        var phase = BulletId % 2 * MathF.PI;
        
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
            var phase = BulletId % 2 * MathF.PI;
            var deflection = _projDesc.Amplitude * MathF.Sin(phase + elapsed / _projDesc.LifetimeMs * _projDesc.Frequency * 2 * MathF.PI);
            origin.X += deflection * MathF.Cos(_angle + MathF.PI / 2);
            origin.Y += deflection * MathF.Sin(_angle + MathF.PI / 2);
        }

        return origin;
    }
}