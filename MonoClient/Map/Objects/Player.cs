using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoClient.Assets.Libraries;
using MonoClient.Assets.XmlStructs;
using MonoClient.Networking;
using MonoClient.Networking.Enums;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects.Enums;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.Rendering;
using MonoClient.Rendering.Types;
using MonoClient.Rendering.Types.SubTypes;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Utils;

namespace MonoClient.Objects;

public class Player : Entity {
    private const float MoveThreshold = 0.4f;
    private const int FocusedSpeed = 15;
    private const float MinMoveSpeed = 0.004f;
    private const float MaxMoveSpeed = 0.0096f;
    private const float MinAttackFreq = 0.0015f;
    private const float MaxAttackFreq = 0.008f;

    private static readonly Logger Log = new(nameof(Player));

    public float Rotate;
    public Vector2 RelativeMoveVector;

    public float MovementMultiplier = 1;

    public bool Focused;

    public int SinkLevel;

    public int Timer;

    public bool Locked;

    public bool Ignored;

    public byte NextBulletId = 1;

    #region StatData

    public int MaxMp;
    public int Mp;
    public int Attack;
    public int Speed;
    public int Dexterity;
    public int Vitality;
    public int Wisdom;

    public int MaxHpBoost;
    public int MaxMpBoost;
    public int AttackBoost;
    public int DefenseBoost;
    public int SpeedBoost;
    public int DexterityBoost;
    public int VitalityBoost;
    public int WisdomBoost;

    public int RandomDex;
    public float AttackPeriod;
    public int AttackStart;

    public int DamageReduction;
    public int DamageIncrease;
    public int AttackSpeedIncrease;
    public int CriticalChance;
    public int CriticalMultiplier;
    public int DodgeChance;
    public int ManaRegenBoost;
    public int ShieldRechargeTime;

    public int AccountId;

    public int Experience;

    public int Stars;

    public int Credits;

    public int Fame;
    public int CurrentFame;
    public int FameGoal;

    public int Souls;

    public bool NameChosen;

    public string Guild;
    public int GuildRank;

    public int OxygenBar;

    public int HealthStackCount;
    public int MagicStackCount;

    public ushort Skin;

    public int Rage;

    public int PartyId;

    public int LdBoosted;
    public int LdBoostAmount;

    public int XpBoostTime;

    public int SkillXpBoostTime;
    public int SkillXpBoostAmount;

    public int DeathBoostTime;
    public int DeathBoost;

    public int SkillPoints;

    public int WargEntity;

    public int SkillExp;
    public int SkillExpGoal;
    public int SkillLevel;

    public int MpCostMult;

    public int PotionShards;

    public int ShieldPoints;
    public int MaxShieldPoints;

    public int MadnessBuildup;

    public int LootBoost;

    public int TrackerId;
    public int TrackerDmg;

    public int DashAmount;
    public int DashTime;

    public bool HasBackPack;

    public bool IsFellowGuild;

    #endregion

    protected override RenderBase GetRenderType(ushort type) {
        ObjectLibrary.TypeToObjectProps.TryGetValue(type, out var props);
        if (props == null) {
            return null;
        }

        if (props.RealSize != -1) {
            Size = props.RealSize;
        }

        if (props.MinSize != props.MaxSize) {
            var maxSteps = (props.MaxSize - props.MinSize) / props.SizeStep;
            Size = props.MinSize + (int) (Random.Shared.NextSingle() * maxSteps) * props.SizeStep;
        }

        return new TypePlayer(this);
    }

    public override bool Update(double time, double dt) {
        if (ObjectId == Map.LocalPlayerId) {
            var angle = Settings.CameraAngle;

            if (Rotate != 0) {
                angle = (float) (angle + dt * Settings.RotateSpeed * Rotate);
                Settings.CameraAngle = (angle % MathHelper.TwoPi + MathHelper.TwoPi) % MathHelper.TwoPi;
            }

            var moveSpeed = GetMoveSpeed();
            var moveVectorAngle = MathF.Atan2(RelativeMoveVector.Y, RelativeMoveVector.X);

            // TODO: Madness debuffs and dashes
            if (RelativeMoveVector.X != 0 || RelativeMoveVector.Y != 0) {
                if (Tile.GroundProperties.SlideAmount > 0) {
                    var slideVector = new Vector2 {
                        X = moveSpeed * MathF.Cos(angle + moveVectorAngle),
                        Y = moveSpeed * MathF.Sin(angle + moveVectorAngle)
                    };

                    var slideLen = slideVector.Length();
                    slideVector *= -1 * (Tile.GroundProperties.SlideAmount - 1);
                    MovementVector *= Tile.GroundProperties.SlideAmount;

                    if (MovementVector.Length() < slideLen) {
                        MovementVector += slideVector;
                    }
                }
                else {
                    MovementVector.X = moveSpeed * MathF.Cos(angle + moveVectorAngle);
                    MovementVector.Y = moveSpeed * MathF.Sin(angle + moveVectorAngle);
                }
            }
            else if (MovementVector.Length() > 0.00012 && Tile.GroundProperties.SlideAmount > 0) {
                MovementVector *= Tile.GroundProperties.SlideAmount;
            }
            else {
                MovementVector.X = 0;
                MovementVector.Y = 0;
            }

            // TODO: Push tiles
            // if (Tile.GroundProperties.Push) {
            //     MovementVector.X = MovementVector.X - Tile.GroundProperties.Animate.Dx / 1000;
            //     MovementVector.Y = MovementVector.Y - Tile.GroundProperties.Animate.Dy / 1000;
            // }

            Timer = (int) time;

            if (IsShooting || Timer < AttackStart + AttackPeriod) {
                AnimationType = AnimationType.Attack;
            }

            if (TextureData.HasAnimationData && this == Map.LocalPlayer) {
                AnimateCharacter();
            }

            WalkTo((float) (Position.X + dt * MovementVector.X), (float) (Position.Y + dt * MovementVector.Y));
            RenderBaseType.SetPosition(Position.X, Position.Y, Z);
        }
        else if (!base.Update(time, dt)) {
            return false;
        }
        Effect?.Update(time, dt);
        return true;
    }

    public override void UpdateStats(List<StatData> statData) {
        base.UpdateStats(statData);

        #region Parse StatData

        foreach (var stat in statData) {
            switch (stat.Type) {
                case StatsType.MaximumMp:
                    MaxMp = stat.Value;
                    break;
                case StatsType.Mp:
                    Mp = stat.Value;
                    break;
                case StatsType.Attack:
                    Attack = stat.Value;
                    break;
                case StatsType.Speed:
                    Speed = stat.Value;
                    break;
                case StatsType.Dexterity:
                    SetDexterity(stat.Value);
                    break;
                case StatsType.Vitality:
                    Vitality = stat.Value;
                    break;
                case StatsType.Wisdom:
                    Wisdom = stat.Value;
                    break;
                case StatsType.HpBoost:
                    MaxHpBoost = stat.Value;
                    break;
                case StatsType.MpBoost:
                    MaxMpBoost = stat.Value;
                    break;
                case StatsType.AttackBonus:
                    AttackBoost = stat.Value;
                    break;
                case StatsType.DefenseBonus:
                    DefenseBoost = stat.Value;
                    break;
                case StatsType.SpeedBonus:
                    SpeedBoost = stat.Value;
                    break;
                case StatsType.DexterityBonus:
                    DexterityBoost = stat.Value;
                    break;
                case StatsType.VitalityBonus:
                    VitalityBoost = stat.Value;
                    break;
                case StatsType.WisdomBonus:
                    WisdomBoost = stat.Value;
                    break;
                case StatsType.AccountId:
                    AccountId = stat.Value;
                    break;
                case StatsType.Experience:
                    Experience = stat.Value;
                    break;
                case StatsType.Stars:
                    Stars = stat.Value;
                    break;
                case StatsType.Credits:
                    Credits = stat.Value;
                    break;
                case StatsType.Fame:
                    Fame = stat.Value;
                    break;
                case StatsType.CurrentFame:
                    CurrentFame = stat.Value;
                    break;
                case StatsType.FameGoal:
                    FameGoal = stat.Value;
                    break;
                case StatsType.NameChosen:
                    NameChosen = stat.Value != 0;
                    break;
                case StatsType.Guild:
                    Guild = stat.Text;
                    break;
                case StatsType.GuildRank:
                    GuildRank = stat.Value;
                    break;
                case StatsType.OxygenBar:
                    OxygenBar = stat.Value;
                    break;
                case StatsType.HealthStackCount:
                    HealthStackCount = stat.Value;
                    break;
                case StatsType.MagicStackCount:
                    MagicStackCount = stat.Value;
                    break;
                case StatsType.Skin:
                    Skin = (ushort)stat.Value;
                    SetPlayerSkinTemplate(Skin);
                    break;
                case StatsType.HasBackpack:
                    HasBackPack = stat.Value != 0;
                    // add backpack signal
                    break;
            }
        }

        #endregion

    }

    private void SetPlayerSkinTemplate(ushort skin) {
        if (skin == 0) return;

        TextureData = ObjectLibrary.TypeToTextureData[skin];
        Texture = TextureData.HasAnimationData ? TextureData.AnimatedTextures.FaceRight[0] : TextureData.GetTexture();
        RenderBaseType.SetTexture(Texture, CurrentFrameIndex == 5);
    }

    private void WalkTo(float x, float y) {
        var pos = ModifyMove(x, y);
        MoveTo(pos.X, pos.Y);

        Camera.Update(pos.X, pos.Y);
    }

    private float AttackFrequency() {
        var attFreq = MinAttackFreq + GetDexterity() / 75f * (MaxAttackFreq - MinAttackFreq);
        attFreq += attFreq * (AttackSpeedIncrease / 100f);
        return attFreq;
    }

    private void SetDexterity(int value) {
        RandomDex = MathUtils.RandomInt(1000, 2500);
        Dexterity = RandomDex + value;
    }

    private int GetDexterity() {
        return Dexterity - RandomDex;
    }

    public void Shoot(float attackAngle) {
        if (Map.LocalPlayer == null) {
            return;
        }

        /*var itemData:ItemData = Equipment[0];
        if (!itemData) {
            return;
        }


        var rateOfFire:Number = itemData.RateOfFire;
        if (itemData.Reforge && itemData.Reforge.ItemType == "Weapon") {
            RateOfFire += itemData.getStatChange(RateOfFire, "RateofFire");
        }*/

        AttackPeriod = 1 / AttackFrequency() * (1 / 1f);

        if (Timer < AttackStart + AttackPeriod) {
            return;
        }

        //this.attackAngle_ = attackAngle;
        AttackStart = Timer;
        IsShooting = true;

        // i cant drag items without it trying to get me to shoot. 
        if (Equipment[0] == null)
            return;
        
        var itemType = Equipment[0].ObjectType;
        var props = ObjectLibrary.TypeToObjectProps[itemType];
        
        var projType = ObjectLibrary.IdToObjectType[props.Projectiles[0].ObjectId];
        var projProps =  ObjectLibrary.TypeToObjectProps[projType];

        for (int i = 0; i < props.NumProjectiles; i++) {
            var arc = MathHelper.ToRadians(props.ArcGap) * (props.NumProjectiles - 1);
            var startAngle = AttackAngle - arc / 2;
            
            var angle = startAngle + MathHelper.ToRadians(props.ArcGap) * i;
            var proj = new Projectile {
                Properties = projProps,
                ProjDesc = props.Projectiles[0],
                Angle = angle,
                StartPosition = Position,
                StartTime = Timer
            };
        
            proj.Owner = this;
            proj.Damage = MathUtils.RandomInt(proj.ProjDesc.MinDamage, proj.ProjDesc.MaxDamage);
        
            proj.SetObjectId(GetBulletId());
            proj.SetType(ObjectLibrary.IdToObjectType[props.Projectiles[0].ObjectId]);
            proj.SetPos(Position.X, Position.Y);
            proj.SetRotation();
            Map.AddProjectile(proj);
            
            var shoot = PlayerShoot.CreatePacket();
            shoot.ContainerType = itemType;
            shoot.BulletId = (byte)proj.ObjectId;
            shoot.Angle = angle;
            shoot.Time = (int)Map.LastGameTime.TotalGameTime.TotalMilliseconds;
            shoot.StartingPos = new Position { X = proj.Position.X, Y = proj.Position.Y };
            
            
            Client.QueuePacket(shoot);
        }
    }

    private Vector2 ModifyMove(float x, float y) {
        var result = new Vector2();

        // if (para and statis dont move) {
        //     result.X = X;
        //     result.Y = Y;
        // }

        var dX = x - Position.X;
        var dY = y - Position.Y;

        if (dX < MoveThreshold && dX > -MoveThreshold && dY < MoveThreshold && dY > -MoveThreshold) {
            result = ModifyStep(x, y);
            return result;
        }

        result.X = Position.X;
        result.Y = Position.Y;

        var stepSize = MoveThreshold / Math.Max(Math.Abs(dX), Math.Abs(dY));
        var d = 0.0f;
        var done = false;

        while (!done) {
            if (d + stepSize >= 1) {
                stepSize = 1 - d;
                done = true;
            }

            result = ModifyStep(result.X + dX * stepSize, result.Y + dY * stepSize);
            d += stepSize;
        }

        return result;
    }

    // Try to keep it as close to the original as possible?
    // Don't wanna mess with it too much.
    // ReSharper disable PossibleLossOfFraction
    // ReSharper disable CompareOfFloatsByEqualityOperator
    private Vector2 ModifyStep(float x, float y) {
        var xCross = Position.X % 0.5f == 0 && x != Position.X || (int) (Position.X / 0.5f) != (int) (x / 0.5f);
        var yCross = Position.Y % 0.5f == 0 && y != Position.Y || (int) (Position.Y / 0.5f) != (int) (y / 0.5f);

        if (!xCross && !yCross || IsValidPosition(x, y)) {
            return new Vector2(x, y);
        }

        float nextXBorder = 0;
        float nextYBorder = 0;

        if (xCross) {
            nextXBorder = x > Position.X ? (int) (x * 2) / 2f : (int) (Position.X * 2) / 2f;

            if ((int) nextXBorder > (int) Position.X) {
                nextXBorder -= 0.01f;
            }
        }

        if (yCross) {
            nextYBorder = y > Position.Y ? (int) (y * 2) / 2f : (int) (Position.Y * 2) / 2f;

            if ((int) nextYBorder > (int) Position.Y) {
                nextYBorder -= 0.01f;
            }
        }

        if (!xCross) {
            if (Tile.GroundProperties.SlideAmount == 0) {
                return new Vector2(x, nextYBorder);
            }

            MovementVector *= -0.5f;
            MovementVector.X *= -1f;

            return new Vector2(x, nextYBorder);
        }

        if (!yCross) {
            if (Tile.GroundProperties.SlideAmount == 0) {
                return new Vector2(nextXBorder, y);
            }

            MovementVector *= -0.5f;
            MovementVector.Y *= -1f;

            return new Vector2(nextXBorder, y);
        }

        var xBorderDist = x > Position.X ? x - nextXBorder : nextXBorder - x;
        var yBorderDist = y > Position.Y ? y - nextYBorder : nextYBorder - y;

        if (xBorderDist > yBorderDist) {
            if (IsValidPosition(x, nextYBorder)) {
                return new Vector2(x, nextYBorder);
            }

            if (IsValidPosition(nextXBorder, y)) {
                return new Vector2(nextXBorder, y);
            }
        }
        else {
            if (IsValidPosition(nextXBorder, y)) {
                return new Vector2(nextXBorder, y);
            }

            if (IsValidPosition(x, nextYBorder)) {
                return new Vector2(x, nextYBorder);
            }
        }

        return new Vector2(nextXBorder, nextYBorder);
    }

    private bool IsValidPosition(float x, float y) {
        var tile = Map.GetTile((int) x, (int) y);

        if (Tile != tile && (tile == null || !tile.IsWalkable())) {
            return false;
        }

        var xFrac = x - (int) x;
        var yFrac = y - (int) y;

        if (xFrac < 0.5) {
            if (IsFullOccupy(x - 1, y)) {
                return false;
            }

            if (yFrac < 0.5) {
                if (IsFullOccupy(x, y - 1) || IsFullOccupy(x - 1, y - 1)) {
                    return false;
                }
            }
            else if (yFrac > 0.5) {
                if (IsFullOccupy(x, y + 1) || IsFullOccupy(x - 1, y + 1)) {
                    return false;
                }
            }
        }
        else if (xFrac > 0.5) {
            if (IsFullOccupy(x + 1, y)) {
                return false;
            }

            if (yFrac < 0.5) {
                if (IsFullOccupy(x, y - 1) || IsFullOccupy(x + 1, y - 1)) {
                    return false;
                }
            }
            else if (yFrac > 0.5) {
                if (IsFullOccupy(x, y + 1) || IsFullOccupy(x + 1, y + 1)) {
                    return false;
                }
            }
        }
        else if (yFrac < 0.5) {
            if (IsFullOccupy(x, y - 1)) {
                return false;
            }
        }
        else if (yFrac > 0.5) {
            if (IsFullOccupy(x, y + 1)) {
                return false;
            }
        }

        return true;
    }

    public void SetRelativeMovement(float rotate, float relMoveVecX, float relMoveVecY) {
        Rotate = rotate;
        RelativeMoveVector.X = relMoveVecX;
        RelativeMoveVector.Y = relMoveVecY;

        if (false) {
            // Confused
            var temp = RelativeMoveVector.X;
            RelativeMoveVector.X = -RelativeMoveVector.Y;
            RelativeMoveVector.Y = -temp;
            Rotate = -Rotate;
        }
    }

    private float GetMoveSpeed() {
        if (false) {
            // Slowed
            return MinMoveSpeed * MovementMultiplier;
        }

        var speed = Focused ? FocusedSpeed : Speed;
        var moveSpeed = MinMoveSpeed + speed / 75 * (MaxMoveSpeed - MinMoveSpeed);

        if (false || false) {
            // Speedy or NinjaSpeedy
            moveSpeed *= 1.5f;
        }

        if (false) {
            // Bunny Speedy
            moveSpeed *= 1.2f;
        }

        return moveSpeed * MovementMultiplier;
    }

    public byte GetBulletId() {
        var ret = NextBulletId;
        NextBulletId = (byte)((NextBulletId + 1) % 128);
        return ret;
    }

    private static bool IsFullOccupy(float x, float y) {
        var tile = Map.GetTile((int) x, (int) y);

        if (tile == null) {
            return true;
        }

        if (tile.Type == 255) {
            return true;
        }

        if (tile.OccupiedObject?.Properties.FullOccupy == true) {
            return true;
        }

        return false;
    }

    public void OnMove() {
        var tile = Map.GetTile((int) Position.X, (int) Position.Y);

        if (tile == null) {
            return;
        }

        // if (tile.GroundProperties.Interactive) {
        //     var activateGround = ActivateGround.CreatePacket();
        //     activateGround.X = tile.X;
        //     activateGround.Y = tile.Y;
        //     Client.QueuePacket(activateGround);
        // }
        //
        // var interactiveObject = Map.GetInteractiveObject((int) Position.X, (int) Position.Y);
        // if (interactiveObject != null) {
        //     var objectInteract = ObjectInteract.CreatePacket();
        //     objectInteract.ObjectId = interactiveObject.ObjectId;
        //     Client.QueuePacket(objectInteract);
        // }

        const float maxSinkLevel = 18;

        if (tile.GroundProperties is { Sinking: true }) {
            // TODO: IMPORTANT! PHARAOH"S CURSE FORCE SINK NEEDS TO BE IMPLEMENTED
            SinkLevel = (int) MathF.Min(SinkLevel + 1, maxSinkLevel);
            MovementMultiplier = 0.1f + (1 - SinkLevel / maxSinkLevel) * (tile.GroundProperties.Speed - 0.1f);
        }
        else {
            SinkLevel = 0;
            MovementMultiplier = tile.GroundProperties.Speed;
        }
    }
}