using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.Assets.Libraries;
using MonoClient.Networking.Enums;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects.Enums;
using MonoClient.Rendering;
using MonoClient.Rendering.Types;
using MonoClient.State;
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

    public int Skin;

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
    public bool HasSatchel;
    public SatchelData SatchelData;

    public AliveMinionsData AliveMinionsData;

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

            if (Properties.AnimatedTexture != null && this == Map.LocalPlayer) {
                AnimateCharacter();
                RenderBaseType.SetTexture(Texture, CurrentFrameIndex == 5);
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
                case StatsType.DamageReduction:
                    DamageReduction = stat.Value;
                    break;
                case StatsType.DamageIncrease:
                    DamageIncrease = stat.Value;
                    break;
                case StatsType.AttackSpeedIncrease:
                    AttackSpeedIncrease = stat.Value;
                    break;
                case StatsType.CriticalChance:
                    CriticalChance = stat.Value;
                    break;
                case StatsType.CriticalMultiplier:
                    CriticalMultiplier = stat.Value;
                    break;
                case StatsType.DodgeChance:
                    DodgeChance = stat.Value;
                    break;
                case StatsType.ManaRegenBoost:
                    ManaRegenBoost = stat.Value;
                    break;
                case StatsType.ShieldRechargeTime:
                    ShieldRechargeTime = stat.Value;
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
                case StatsType.Souls:
                    Souls = stat.Value;
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
                    Skin = stat.Value;
                    SetPlayerSkinTemplate(Skin);
                    break;
                case StatsType.Rage:
                    Rage = stat.Value;
                    break;
                case StatsType.PartyId:
                    PartyId = stat.Value;
                    break;
                case StatsType.LdBoosted:
                    LdBoosted = stat.Value;
                    break;
                case StatsType.LdBoostAmount:
                    LdBoostAmount = stat.Value;
                    break;
                case StatsType.XpBoosted:
                    XpBoostTime = stat.Value;
                    break;
                case StatsType.SkillXpBoostTime:
                    SkillXpBoostTime = stat.Value;
                    break;
                case StatsType.SkillXpBoostAmount:
                    SkillXpBoostAmount = stat.Value;
                    break;
                case StatsType.DeathBoostTime:
                    DeathBoostTime = stat.Value;
                    break;
                case StatsType.DeathBoost:
                    DeathBoost = stat.Value;
                    break;
                case StatsType.SkillPoints:
                    SkillPoints = stat.Value;
                    break;
                case StatsType.WargEntity:
                    break;
                case StatsType.SkillExp:
                    SkillExp = stat.Value;
                    break;
                case StatsType.SkillExpGoal:
                    SkillExpGoal = stat.Value;
                    break;
                case StatsType.SkillLevel:
                    SkillLevel = stat.Value;
                    break;
                case StatsType.MpCostMult:
                    MpCostMult = stat.Value;
                    break;
                case StatsType.PotionShards:
                    PotionShards = stat.Value;
                    break;
                case StatsType.ShieldPoints:
                    ShieldPoints = stat.Value;
                    break;
                case StatsType.MaxShieldPoints:
                    MaxShieldPoints = stat.Value;
                    break;
                case StatsType.MadnessBuildup:
                    MadnessBuildup = stat.Value;
                    break;
                case StatsType.LootBoostStat:
                    LootBoost = stat.Value;
                    break;
                case StatsType.HasBackpack:
                    HasBackPack = stat.Value != 0;
                    // add backpack signal
                    break;
                case StatsType.HasMaterialSatchel:
                    HasSatchel = stat.Value != 0;
                    // add satchel signal
                    break;
                case StatsType.MaterialSatchel:
                    SatchelData = (SatchelData) stat.DataObject;
                    break;
                case StatsType.AliveMinions:
                    AliveMinionsData = (AliveMinionsData) stat.DataObject;
                    break;
                case StatsType.TrackerId:
                    TrackerId = stat.Value;
                    break;
                case StatsType.TrackerDmg:
                    TrackerDmg = stat.Value;
                    break;
                case StatsType.DashAmount:
                    DashAmount = stat.Value;
                    DashTime = 0; // dash time current time
                    break;
            }
        }

        #endregion

    }

    private void SetPlayerSkinTemplate(int skin) {
        if (skin != 0) {
            var props = ObjectLibrary.TypeToObjectProps[(ushort) skin];
            var texture = props.AnimatedTexture;
            Frames = Main.Atlas.AtlasMapAnimation[texture.File][texture.Index];
            Texture = Frames.FaceRight[0];
        }
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
        //this.map_.gs_.gsc_.playerShoot(this.doShoot(this.attackStart_, this.attackAngle_, true, itemData, ProjectileType.SHOOT), 0, ExplodeType.NONE, false, itemData.ObjectType);
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
            nextXBorder = x > Position.X ? (int) (x * 2) / 2 : (int) (Position.X * 2) / 2;

            if ((int) nextXBorder > (int) Position.X) {
                nextXBorder -= 0.01f;
            }
        }

        if (yCross) {
            nextYBorder = y > Position.Y ? (int) (y * 2) / 2 : (int) (Position.Y * 2) / 2;

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