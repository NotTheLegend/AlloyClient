using System;
using System.Runtime.InteropServices;
using Alloy.Common;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game.Objects.ProjectilePaths;
using AlloyClient.Game.Objects.Util;
using AlloyClient.Networking;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.ParticleEffects;
using AlloyClient.Rendering;
using AlloyClient.Ui.Character;
using AlloyClient.Utils;
using Alloy.Common.Structs;
using Alloy.Engine;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Objects;

public readonly struct ProjectileKey(int entityId, uint id) {
    public readonly ulong Key = ((ulong)entityId << 32) | id;
    public int EntityId => (int)(Key >> 32);
    public uint Id => (uint)Key;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EntityTexture(AtlasData uv, Vector4 scale) { // TODO: move this elsewhere
    public readonly AtlasData UV = uv;
    public readonly Vector4 Scale = scale;
    
    // TODO: redo this logic, tis a jank unreadable mess

    public static EntityTexture Create(AtlasData texture, bool attackFrame = false) {
        var frameMult = attackFrame ? 2f : 1f;
        var w = texture.RawW() - AtlasData.Padding * 2;
        var h = texture.RawH() - AtlasData.Padding * 2;
        
        // this should be padding * 2 but the attack frame doesnt line up unless its 3 for some fucking reason
        var padW = 1.0f + AtlasData.Padding * 3 / texture.RawW();
        var padH = 1.0f + AtlasData.Padding * 3 / texture.RawH();
        
        var ratio = w / h / frameMult * MathF.Max(w / frameMult / 8, h / 8);

        var widthScale = 0.75f * ratio * frameMult * padW;
        var heightScale = 0.75f * ratio * padH;
        
        var padX = attackFrame ? widthScale * (0.5f - (AtlasData.Padding + w / 4) / texture.RawW()) : 0f;
        var padY = heightScale * (0.5f - AtlasData.Padding / texture.RawH());

        var scale = new Vector4(widthScale, heightScale, padX, -padY);
        return new EntityTexture(texture, scale);
    }
    
    public static EntityTexture CreateCentered(AtlasData texture, bool attackFrame = false) {
        var frameMult = attackFrame ? 2f : 1f;
        var w = texture.RawW() - AtlasData.Padding * 2;
        var h = texture.RawH() - AtlasData.Padding * 2;
        
        // this should be padding * 2 but the attack frame doesnt line up unless its 3 for some fucking reason
        var padW = 1.0f + AtlasData.Padding * 3 / texture.RawW();
        var padH = 1.0f + AtlasData.Padding * 3 / texture.RawH();
        
        var ratio = w / h / frameMult * MathF.Max(w / frameMult / 8, h / 8);

        var widthScale = 0.75f * ratio * frameMult * padW;
        var heightScale = 0.75f * ratio * padH;

        var scale = new Vector4(widthScale, heightScale, 0, 0);
        return new EntityTexture(texture, scale);
    }
}

public sealed class Projectile : IResettable { // TODO: make struct

    private const double HitTestDelayMs = 16;
    
    // TODO: do something about proj paths
    public ProjectilePath Path;
    private Vector2 _startPosition;
    
    
    /*===== new fields =======*/
    private ProjectileKey _key; // readonly
    private int _damage; // readonly
    private bool _damagePlayers; // readonly
    private EntityTexture _texture; // readonly
    private float _size; // readonly
    private float _angleCorrection; // readonly
    private float _rotationSpeed; // readonly
    private bool _multiHit; // readonly
    private bool _passesCover; // readonly
    private bool _noRotation; // readonly
    private bool _hasTrail; // readonly
    private ParticleTrail _particleTrail; // readonly

    private float _rotation;
    private float _elapsed; // double
    private Vector2 _position;

    public void Reset(ushort id, int dmg, float angle, Entity entity, ObjectProperties objDesc, ProjectileProperties projDesc, ProjectilePath path, Vector2 startPos) {
        Path = path ?? projDesc.Path.Clone();
        Path.SetInfo(new ProjectileInfo() { LifetimeMs = Path.LifetimeMs, ProjId = id, ShootAngle = angle * MathHelper.DegToRad, StartPos = startPos});
        _position = _startPosition = startPos;
        
        /*===== new =====*/
        _key = new ProjectileKey(entity.ObjectId, id);
        _damage = dmg;
        _damagePlayers = entity is not Player;
        _texture = EntityTexture.CreateCentered(GetTexture(objDesc.ObjectType));
        _size = (projDesc.Size > 0 ? projDesc.Size : 100) / 100f;
        _angleCorrection = objDesc.AngleCorrection * MathHelper.PiOver4;
        _rotationSpeed = objDesc.Rotation;
        _multiHit = projDesc.MultiHit;
        _passesCover = projDesc.PassesCover;
        _noRotation = projDesc.NoRotation;
        _hasTrail = projDesc.HasParticleTrail;
        _particleTrail = projDesc.ParticleTrail;
    }

    public bool IsInPool { get; set; }

    public void Reset() {
        Path = null;
        
        /* === temp === */
        _elapsed = 0;
    }

    private static AtlasData GetTexture(ushort objType) {
        var textureData = ObjectLibrary.TypeToTextureData[objType];
        return textureData.HasAnimationData ? textureData.AnimatedTextures.FaceRight[0] : textureData.GetTexture();
    }

    public bool Update(in GameTime gameTime) {
        _elapsed += (float)gameTime.ElapsedMs;

        if (_elapsed > Path.LifetimeMs) {
            return false;
        }

        var deltaPos = Path.PositionAt(_elapsed);
        var newPos = _startPosition + deltaPos;
        
        if (_rotationSpeed != 0) {
            _rotation = _elapsed / _rotationSpeed;
        } else if (!_noRotation) {
            var direction = newPos - _position;
            var angle = MathF.Atan2(direction.Y, direction.X);
            _rotation = angle + Settings.CameraAngle + _angleCorrection;
        }

        
        return MoveTo(newPos);
    }

    public void FixedUpdate(in GameTime gameTime) {
        if (HitTest(gameTime.TotalMs)) {
            _elapsed = float.MaxValue;
            return;
        }
        
        if (_hasTrail) {
            Map.AddParticleEffect(new SparkEffect(100, _particleTrail.Color, _particleTrail.LifetimeMs, 0.5f, Random.Shared.PlusMinus(3f), Random.Shared.PlusMinus(3f), _position.X, _position.Y));
            Map.AddParticleEffect(new SparkEffect(100, _particleTrail.Color, _particleTrail.LifetimeMs, 0.5f, Random.Shared.PlusMinus(3f), Random.Shared.PlusMinus(3f), _position.X, _position.Y));
            Map.AddParticleEffect(new SparkEffect(100, _particleTrail.Color, _particleTrail.LifetimeMs, 0.5f, Random.Shared.PlusMinus(3f), Random.Shared.PlusMinus(3f), _position.X, _position.Y));
        }
    }

    public VertexObject Draw(in DepthMatrix matrix) {
        var s = MathF.Sin(-_rotation);
        var c = MathF.Cos(-_rotation);
        var jitter = (_key.Key * 0.00001f) % 0.01f;
        var sort = 0.5f + 0.4f * (_position.X * matrix.M12 + _position.Y * matrix.M22 + matrix.M42) + jitter;
        return new VertexObject(new Vector3(_position, 0.5f), _texture.UV.ToVector4(), _texture.Scale, new Vector4(s, c, _size, -1f), ExtraData.NewShadedObject(sort, 1f), Color.Black);
    }

    public ShadowData DrawShadow() => new (_position, 0.5f, Color.Black);
    
    private bool MoveTo(Vector2 pos) {
        if (Vector2.Truncate(pos) != Vector2.Truncate(_position)) {
            var tile = Map.LookupTile(pos);

            if (tile == null || tile.Type == 0xFF) {
                return false; // TODO: hit effect
            }
        
            if (tile.OccupiedObject != null) {
                var obj = tile.OccupiedObject.Properties;
                if ((!obj.IsEnemy || _damagePlayers) && (obj.EnemyOccupySquare || !_passesCover && obj.OccupySquare)) {
                    return false; // TODO: hit effect
                }
            }
        }
        
        _position = pos;
        return true;
    }
    
    private bool HitTest(double time) {
        if (_damagePlayers) {

            var target = EntityUtils.GetClosestPlayer(_position, 0.5f);

            if (target == null || target.MultiHitUsed.ContainsKey(_key)) {
                return false;
            }
            
            Map.AddParticleEffect(new HitEffect(target, 0xFF0000));
            NotificationLayer.AddStatusText(target, $"-{_damage}", 0xFF0000, 1000, 0);
            
            var hit = PlayerHit.CreatePacket();
            hit.BulletId = (ushort)_key.Id;
            hit.ObjectId = _key.EntityId;
            
            Client.QueuePacket(hit);

            if (!_multiHit) {
                return true;
            }

            target.MultiHitUsed.Add(_key, time + Path.LifetimeMs);
            return false;
        }

        var enemy = EntityUtils.GetClosestEnemy(_position, 0.5f);

        if (enemy == null || enemy.MultiHitUsed.ContainsKey(_key)) {
            return false;
        }

        Map.AddParticleEffect(new HitEffect(enemy, 0xFF0000));
        NotificationLayer.AddStatusText(enemy, $"-{_damage}", 0xFF0000, 1000, 0);
        
        var hit1 = EnemyHit.CreatePacket();
        hit1.BulletId = (ushort)_key.Id;
        hit1.TargetId = enemy.ObjectId;
        
        Client.QueuePacket(hit1);
        
        if (!_multiHit) {
            return true;
        }

        enemy.MultiHitUsed.Add(_key, time + Path.LifetimeMs);
        return false;
    }
}