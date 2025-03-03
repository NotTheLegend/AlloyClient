using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Assets;
using MonoClient.Assets.Libraries;
using MonoClient.Assets.XmlStructs;
using MonoClient.Networking;
using MonoClient.Networking.Enums;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Rendering;
using MonoClient.Rendering.Types;
using MonoClient.Objects.Enums;
using MonoClient.Objects.Util;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.ParticleEffects;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Ui.Character;
using MonoClient.UiLib.Utils.Signals;
using MonoClient.Utils;
using Newtonsoft.Json;

namespace MonoClient.Objects;

public class Entity {
    private static readonly Logger Log = new(nameof(Entity));

    public const float AttackPeriod = 100;

    public int ObjectId;
    public ushort Type;
    
    public Signal<int> InventoryUpdate = new();

    public float HeightOffset;
    public Vector2 Position;
    public float Rotation;
    public float Z;

    public Vector2 MovementVector;
    public Vector2 TickPosition;
    public Vector2 PositionAtTick;

    public int LastTickId;
    public double LastTickUpdateTime;

    public MapTile Tile;
    public ObjectProperties Properties;

    #region StatData

    public string Name;

    public int Texture1Id;
    public int Texture2Id;

    public int AttackStart;
    public float AttackAngle;
    public bool IsShooting;

    public int GlowColor;

    public int MaxHp;
    public int Hp;
    public int Defense;

    public int Size = 100;

    public int Level;

    public ItemDesc[] Equipment = new ItemDesc[20];

    public ConditionEffects ConditionEffects = 0;

    public int ConnectType;

    public int PlayerHost;

    public bool PermaPet;

    public int MadnessPullRadius;
    public int MadnessPullMaxSpeed;

    public int QuestGlowColor;

    public int MinimumHp;

    public bool DontFaceAttacks;

    public bool AllDebuffsImmune;

    public int DamagersCount;

    public int CustomTexture;

    public bool PortalUsable;

    #endregion

    public int Timer;

    public RenderBase RenderBaseType;

    public TextureData TextureData;

    public AtlasData Texture;

    public AnimationType AnimationType;

    public FaceDirection LocalFaceDirection = FaceDirection.None;

    public Stopwatch AnimationTimer = new();
    public int CurrentFrameIndex;
    public bool Flipped;
    
    public float Jitter;

    public ParticleEffect Effect;

    public void SetObjectId(int id) {
        ObjectId = id;
        Jitter = Random.Shared.NextSingle() * 0.00002f - 0.00001f;
        Effect = ParticleEffect.FromProperties(Properties.Effect, this);
    }

    public void SetType(ushort type) {
        Type = type;
        TextureData = ObjectLibrary.TypeToTextureData[type];
        Texture = TextureData.HasAnimationData ? TextureData.AnimatedTextures.FaceRight[0] : TextureData.GetTexture();
        RenderBaseType = GetRenderType(type);
    }

    public Color GetDominateColor() {
        if (RenderBaseType is TypeWall) return TextureData.TopTexture.DominantColor;
        return TextureData.DominantColor;
    }

    protected virtual RenderBase GetRenderType(ushort type) {
        ObjectLibrary.TypeToObjectProps.TryGetValue(type, out var props);

        if (props == null) {
            return new TypeNullObject();
        }

        if (props.RealSize != -1) {
            Size = props.RealSize;
        }

        if (props.MinSize != props.MaxSize) {
            var maxSteps = (props.MaxSize - props.MinSize) / props.SizeStep;
            Size = props.MinSize + (int)(Random.Shared.NextSingle() * maxSteps) * props.SizeStep;
        }

        if (!string.IsNullOrEmpty(props.Model)) {
            return new TypeModel3D(props.Model, this);
        }
        
        if (props.DrawOnGround) {
            return new TypeGroundObject(this);
        }

        return props.Class switch {
            "Wall" => new TypeWall(this),
            "DoubleWall" => new TypeWall(this, ModelType.PbDoubleWall),
            "DoubleWall2" => new TypeWall(this, ModelType.PbDoubleWall), // ModelType.PbDoubleWall2
            "TripleWall" => new TypeWall(this, ModelType.PbDoubleWall), // ModelType.PbTripleWall
            _ => new TypeGameObject(this)
        };
    }

    public void SetPos(float x, float y) {
        Position.X = x;
        Position.Y = y;

        Rotation = Properties.Rotation;
        RenderBaseType.SetPosition(x, y);
    }

    public bool HasConditionEffect(ConditionEffects effect) {
        return (ConditionEffects & effect) != 0;
    }

    public virtual bool Update(double time, double dt) {
        if (Settings.MovementInterpolation) {
            var dx = TickPosition.X - Position.X;
            var dy = TickPosition.Y - Position.Y;
            var distSqr = dx * dx + dy * dy;

            if (distSqr > 0.0001) {
                var tickDt = dt * 0.004;
                var pX = tickDt * TickPosition.X + (1 - tickDt) * Position.X;
                var pY = tickDt * TickPosition.Y + (1 - tickDt) * Position.Y;
                MoveTo((float)pX, (float)pY);

                AnimationType = AnimationType.Walk;
            }
            else {
                MovementVector.X = 0;
                MovementVector.Y = 0;
                AnimationType = AnimationType.Stand;
            }
        }
        else {
            if (MovementVector is not { X: 0, Y: 0 }) {
                if (LastTickId >= Map.LastTickId) {
                    var tickDt = time - LastTickUpdateTime;
                    var pX = PositionAtTick.X + tickDt * MovementVector.X;
                    var pY = PositionAtTick.Y + tickDt * MovementVector.Y;
                    MoveTo((float)pX, (float)pY);
                }
                else {
                    MovementVector.X = 0;
                    MovementVector.Y = 0;
                    MoveTo(TickPosition.X, TickPosition.Y);
                }
            }
            else {
                MovementVector.X = 0;
                MovementVector.Y = 0;
            }
        }

        Timer = (int)time;

        if (IsShooting || Timer < AttackStart + AttackPeriod) {
            AnimationType = AnimationType.Attack;
        }

        // add wall support here at some point
        if (TextureData.HasAnimationData) {
            AnimateCharacter();
        }
        
        Effect?.Update(time, dt);

        RenderBaseType.SetPosition(Position.X, Position.Y, Z);
        return true;
    }

    public void UpdateVisibility(ref Matrix matrix) {
        var dx = Position.X - Camera.Position.X;
        var dy = Position.Y + Camera.Position.Y;
        var distanceSquared = dx * dx + dy * dy;
        const int playerSightRadiusSquared = Map.TileRenderDistance * Map.TileRenderDistance;
        RenderBaseType.SetVisibility(distanceSquared <= playerSightRadiusSquared);
        
        var sort = Vector3.Transform(new Vector3(Position.X, Position.Y, 0), matrix).Y;
        RenderBaseType.SetDepth(0.5f + 0.4f * sort + Jitter);
    }

    public bool MoveTo(float x, float y) {
        var tile = Map.GetTile((int)x, (int)y);

        if (tile == null) {
            return false;
        }
        
        if (tile.OccupiedObject != null && tile.OccupiedObject.Properties.OccupySquare) {
            return false;
        }

        Position.X = x;
        Position.Y = y;

        if (Properties.Static) {
            if (Tile != null) {
                Tile.OccupiedObject = null;
            }

            tile.OccupiedObject = this;
        }

        Tile = tile;

        return true;
    }

    public bool HitTest(Projectile proj) {
        if (proj.Owner is Player) {
            var enemyTarget = EntityUtils.FindClosestEnemyInRadius(proj, Map.Entities.Values, 0.5f);
            if (enemyTarget != null) {
                enemyTarget.Hp -= proj.Damage;
                
                enemyTarget.Effect = new HitEffect(enemyTarget, 0xff0000);
                NotificationLayer.AddStatusText(enemyTarget, $"-{proj.Damage}", 0xFF0000, 1000, 0);
                
                var hit = EnemyHit.CreatePacket();
                hit.Time = (int)Map.LastGameTime.TotalGameTime.TotalMilliseconds;
                hit.BulletId = (byte)proj.ObjectId;
                hit.TargetId = enemyTarget.ObjectId;
                hit.Killed = enemyTarget.Hp <= proj.Damage;

                Client.QueuePacket(hit);
                return true;
            }
        } 
        else {
            var target = EntityUtils.FindClosestPlayerInRadius(proj, Map.Entities.Values, 0.5f);
            if (target != null) {
                target.Effect = new HitEffect(target, 0xff0000);
                NotificationLayer.AddStatusText(target, $"-{proj.Damage}", 0xFF0000, 1000, 0);

                var hit = PlayerHit.CreatePacket();
                hit.BulletId = (byte)proj.ObjectId;
                hit.ObjectId = proj.Owner.ObjectId;
            
                Client.QueuePacket(hit);
                return true;
            }
        }
        return false;
    }

    public void OnTickPosition(float x, float y, double tickTime, int tickId, bool isPlayer) {
        if (!Settings.MovementInterpolation && LastTickId < Map.LastTickId && !isPlayer) {
            MoveTo(TickPosition.X, TickPosition.Y);
        }

        TickPosition.X = x;
        TickPosition.Y = y;
        LastTickId = tickId;
        LastTickUpdateTime = tickTime;
        PositionAtTick.X = Position.X;
        PositionAtTick.Y = Position.Y;

        if (!isPlayer) {
            MovementVector.X = (float)((TickPosition.X - PositionAtTick.X) / tickTime);
            MovementVector.Y = (float)((TickPosition.Y - PositionAtTick.Y) / tickTime);
        }
    }

    public virtual void UpdateStats(List<StatData> statData) {
        foreach (var stat in statData) {
            switch (stat.Type) {
                case StatsType.MaximumHp:
                    MaxHp = stat.Value;
                    break;
                case StatsType.Hp:
                    Hp = stat.Value;
                    break;
                case StatsType.Defense:
                    Defense = stat.Value;
                    break;
                case StatsType.Size:
                    Size = stat.Value;
                    break;
                case StatsType.Level:
                    Level = stat.Value;
                    break;
                case StatsType.Inventory0:
                case StatsType.Inventory1:
                case StatsType.Inventory2:
                case StatsType.Inventory3:
                case StatsType.Inventory4:
                case StatsType.Inventory5:
                case StatsType.Inventory6:
                case StatsType.Inventory7:
                case StatsType.Inventory8:
                case StatsType.Inventory9:
                case StatsType.Inventory10:
                case StatsType.Inventory11:
                    var index = stat.Type - StatsType.Inventory0;
                    if (stat.Value == -1) {
                        Equipment[index] = null;
                    }
                    else if (Equipment[index] == null || (Equipment[index] != null && stat.Value != ((ItemDesc) Equipment[index]).ObjectType)) {
                        Equipment[index] = ObjectLibrary.CreateItem((ushort)stat.Value);
                    }
                    InventoryUpdate.Dispatch(index);
                    break;
                case StatsType.Effects:
                    ConditionEffects = (ConditionEffects)stat.Value;
                    break;
                case StatsType.Name:
                    Name = stat.Text;
                    RenderBaseType.SetName(stat.Text);
                    break;
                case StatsType.Texture1:
                    if (stat.Value == Texture1Id) {
                        break;
                    }

                    Texture1Id = stat.Value;
                    // TexturingCache = new Dict;
                    // Portrait = null;
                    break;
                case StatsType.Texture2:
                    if (stat.Value == Texture2Id) {
                        break;
                    }

                    Texture2Id = stat.Value;
                    // TexturingCache = new Dict;
                    // Portrait = null;
                    break;
                case StatsType.Glow:
                    GlowColor = stat.Value;
                    break;
                case StatsType.AltTextureIndex:
                    SetAltTexture(stat.Value);
                    break;
                case StatsType.BackPack0:
                case StatsType.BackPack1:
                case StatsType.BackPack2:
                case StatsType.BackPack3:
                case StatsType.BackPack4:
                case StatsType.BackPack5:
                case StatsType.BackPack6:
                case StatsType.BackPack7:
                    index = 12 + stat.Type - StatsType.BackPack0;
                    if (stat.Value == -1) {
                        Equipment[index] = null;
                    }
                    else if (Equipment[index] == null || (Equipment[index] != null && stat.Value != ((ItemDesc) Equipment[index]).ObjectType)) {
                        Equipment[index] = ObjectLibrary.CreateItem((ushort)stat.Value);
                    }
                    InventoryUpdate.Dispatch(index);
                    break;
                case StatsType.HasBackpack:
                    //todo
                    break;
                case StatsType.PortalUsable:
                    PortalUsable = stat.Value != 0;
                    break;
            }
        }
    }

    //public virtual void Draw() {
    //    Renderer?.Render();
    //}

    public virtual void Reset() {
        ObjectId = 0;

        Position.X = 0;
        Position.Y = 0;
        Z = 0;

        MovementVector.X = 0;
        MovementVector.Y = 0;

        Tile = null;
    }

    public AtlasData GetTexture() {
        return Texture;
    }

    public void SetAttack(int containerType, float angleInc) {
        AttackStart = Timer;
    }

    // we should move this shit somewhere else too
    public void AnimateCharacter() {
        var dx = TickPosition.X - Position.X;
        var dy = TickPosition.Y - Position.Y;

        var localPlayer = this == Map.LocalPlayer;
        var movementAngle = localPlayer ? MathF.Atan2(MovementVector.Y, MovementVector.X) : MathF.Atan2(dy, dx);
        var camFlipped = Math.Abs(Settings.CameraAngle - MathHelper.Pi) < MathHelper.PiOver2;
        var moveFlipped = Math.Abs(movementAngle) > MathHelper.PiOver2;
        var isPlayer = this is Player;// facing right/down no idea what to call the var

        Flipped = (!camFlipped && moveFlipped || (camFlipped && !moveFlipped)) && !isPlayer;
        var directionalIndex = isPlayer ? GetCharacterFrameIndex(movementAngle, Settings.CameraAngle, localPlayer) : 0;

        switch (AnimationType) {
            case AnimationType.Stand:
                Texture = GetFrameData(directionalIndex, 0);
                CurrentFrameIndex = 0;
                break;
            case AnimationType.Walk:
                if (!AnimationTimer.IsRunning) {
                    AnimationTimer.Start();
                }

                var distSqr = dx * dx + dy * dy;
                var duration = 200; // - distSqr * 10;

                if (AnimationTimer.ElapsedMilliseconds > duration) {
                    CurrentFrameIndex = CurrentFrameIndex == 1 ? 2 : 1;

                    Texture = GetFrameData(directionalIndex, CurrentFrameIndex);
                    
                    if (Texture.W == 0 || Texture.H == 0) // some sheets have a blank frame 2
                        Texture = GetFrameData(directionalIndex, 0);
                    
                    AnimationTimer.Restart();
                }

                break;
            case AnimationType.Attack:
                if (!AnimationTimer.IsRunning) {
                    AnimationTimer.Start();
                }

                float timer = 5000;

                if (this is Player player) {
                    if (player.Timer < player.AttackStart + player.AttackPeriod) {
                        timer = (player.Timer - player.AttackStart) % player.AttackPeriod / player.AttackPeriod;
                    }
                    else
                        player.IsShooting = false;
                }
                else {
                    if (Timer < AttackStart + AttackPeriod) {
                        timer = (Timer - AttackStart) % AttackPeriod / AttackPeriod;
                    }
                    else
                        IsShooting = false;
                }

                if (AnimationTimer.ElapsedMilliseconds > timer * 1000) {
                    CurrentFrameIndex = CurrentFrameIndex == 4 ? 5 : 4;

                    Texture = GetFrameData(directionalIndex, CurrentFrameIndex);
                    
                    AnimationTimer.Restart();
                }

                break;
        }
        
        RenderBaseType.SetTexture(Texture, CurrentFrameIndex == 5);
    }

    public AtlasData GetFrameData(int direction, int frame) {
        return direction switch {
            0 => TextureData.AnimatedTextures.FaceRight[frame], // Right/Left
            1 => TextureData.AnimatedTextures.FaceDown[frame], // Down
            2 => TextureData.AnimatedTextures.FaceUp[frame], // Up
            _ => TextureData.AnimatedTextures.FaceRight[0]
        };
    }

    private int GetCharacterFrameIndex(float movementAngle, float cameraAngle, bool localPlayer) {
        Flipped = false;
        var correctedFacingAngle = MathHelper.WrapAngle(movementAngle - cameraAngle);
        var rotationAngle = (MathHelper.ToDegrees(correctedFacingAngle) + 360) % 360;

        if (IsShooting) {
            return GetDirectionFromAttackAngle(AttackAngle, Settings.CameraAngle.Value);
        }
        
        if (LocalFaceDirection != FaceDirection.None && !InputHandler.Moving) {
            switch (LocalFaceDirection) {
                case FaceDirection.Right:
                    return 0;
                case FaceDirection.Left:
                    Flipped = true;
                    return 0;
                case FaceDirection.Down:
                    return 1;
                case FaceDirection.Up:
                    return 2;
            }
        }

        switch ((int)rotationAngle) {
            case < 45 or >= 315:
                if (localPlayer)
                    LocalFaceDirection = FaceDirection.Right;

                return 0; // right

            case >= 135 and < 225:
                if (localPlayer)
                    LocalFaceDirection = FaceDirection.Left;

                Flipped = true;
                return 0; // left

            case >= 45 and < 135:
                if (localPlayer)
                    LocalFaceDirection = FaceDirection.Down;

                return 1; // down

            case >= 225 and < 315:
                if (localPlayer)
                    LocalFaceDirection = FaceDirection.Up;

                return 2; // up
        }
    }

    private int GetDirectionFromAttackAngle(float attackAngle, float cameraAngle) {
        float relativeAttackAngle = attackAngle - cameraAngle;
        relativeAttackAngle = MathUtils.NormalizeAngle(relativeAttackAngle);

        switch (relativeAttackAngle) {
            case >= -MathF.PI / 4 and < MathF.PI / 4:
                return 0;
            case >= MathF.PI / 4 and < 3 * MathF.PI / 4:
                return 1;
            case >= -3 * MathF.PI / 4 and < -MathF.PI / 4:
                return 2; 
            default:
                Flipped = true;
                return 0;
        }
    }

    private void SetAltTexture(int index) {
    }

    public void OnAddedToMap(Position position) {
        PositionAtTick = Position;
        TickPosition = Position;

        if (!MoveTo(position.X, position.Y)) {
            Log.Error($"Failed to add entity {ObjectId} to map.");
        }

        // Add entity's effect
    }

    public void OnRemovedFromMap() {
        if (Properties.Static && Tile != null) {
            if (Tile.OccupiedObject == this) {
                Tile.OccupiedObject = null;
            }

            Tile = null;
        }

        // Remove entity's effect

        // Remove dmg counter

        // Clear Madness dict

        // Dispose
    }
}