using System;
using System.Collections.Generic;
using System.Linq;
using AlloyClient.Logging;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Objects.Util;

public sealed class ConditionEffect(string name, ConditionEffects bit, int[] iconOffsets) {
    public readonly string Name = name;
    public readonly ConditionEffects Bit = bit;
    public readonly int[] IconOffsets = iconOffsets;
}

public static class ConditionEffectUtil {

    private static readonly ILogger Logger = ILogger.CreateLogger(nameof(ConditionEffect));
    
    public static readonly ConditionEffect[] Effects = [
        new("Nothing", ConditionEffects.None, null),
        new("Quiet", ConditionEffects.Quiet, [32]),
        new("Weak", ConditionEffects.Weak, [34, 35, 36, 37]),
        new("Slowed", ConditionEffects.Slowed, [1]),
        new("Sick", ConditionEffects.Sick, [39]),
        new("Dazed", ConditionEffects.Dazed, [44]),
        new("Stunned", ConditionEffects.Stunned, [45]),
        new("Blind", ConditionEffects.Blind, [41]),
        new("Hallucinating", ConditionEffects.Hallucinating, [42]),
        new("Drunk", ConditionEffects.Drunk, [43]),
        new("Confused", ConditionEffects.Confused, [2]),
        new("Stun Immune", ConditionEffects.StunImmune, null),
        new("Invisible", ConditionEffects.Invisible, null),
        new("Paralyzed", ConditionEffects.Paralyzed, [53, 54]),
        new("Speedy", ConditionEffects.Speedy, [0]),
        new("Bleeding", ConditionEffects.Bleeding, [46]),
        new("Healing", ConditionEffects.Healing, [47]),
        new("Damaging", ConditionEffects.Damaging, [49]),
        new("Berserk", ConditionEffects.Berserk, [50]),
        new("Paused", ConditionEffects.Paused, null),
        new("Stasis", ConditionEffects.Stasis, null),
        new("Stasis Immune", ConditionEffects.StasisImmune, null),
        new("Invincible", ConditionEffects.Invincible, null),
        new("Invulnerable", ConditionEffects.Invulnerable, [17]),
        new("Armored", ConditionEffects.Armored, [16]),
        new("Armor Broken", ConditionEffects.ArmorBroken, [55]),
        new("Hexed", ConditionEffects.Hexed, [42]),
        new("Ninja Speedy", ConditionEffects.NinjaSpeedy, [0])
    ];

    public static readonly ConditionEffects IconlessEffects;

    public static readonly Dictionary<ConditionEffects, Vector4[]> EffectIcons = [];

    private static readonly Dictionary<string, ConditionEffects> NameToEffect = [];

    static ConditionEffectUtil() {
        foreach (var effect in Effects) {
            if (effect.IconOffsets != null)
                EffectIcons[effect.Bit] = effect.IconOffsets.Select(i => Main.Atlas.GetAtlasData("lofiInterface2", i).ToVector4()).ToArray();
            else
                IconlessEffects |= effect.Bit;
            
            NameToEffect[effect.Name] = effect.Bit;
        }
    }

    public static int GetConditionEffectId(string name) {
        if (NameToEffect.TryGetValue(name, out var eff))
            return (int)eff;
        
        Logger.Log(LogLevel.Warning, $"Unable to find effect: {name}");
        return (int)ConditionEffects.None;
    }

    public static bool IsNegativeCondition(ConditionEffectIndex effect) {
        return effect switch {
            ConditionEffectIndex.Quiet => true,
            ConditionEffectIndex.Weak => true,
            ConditionEffectIndex.Slowed => true,
            ConditionEffectIndex.Sick => true,
            ConditionEffectIndex.Dazed => true,
            ConditionEffectIndex.Stunned => true,
            ConditionEffectIndex.Blind => true,
            ConditionEffectIndex.Hallucinating => true,
            ConditionEffectIndex.Drunk => true,
            ConditionEffectIndex.Confused => true,
            ConditionEffectIndex.Paralyzed => true,
            ConditionEffectIndex.Bleeding => true,
            ConditionEffectIndex.Stasis => true,
            ConditionEffectIndex.ArmorBroken => true,
            ConditionEffectIndex.Hexed => true,
            _ => false
        };
    }

    public static ConditionEffectIndex GetImmuneEffectIndex(ConditionEffectIndex effect) {
        return effect switch {
            ConditionEffectIndex.StasisImmune => ConditionEffectIndex.Stasis,
            ConditionEffectIndex.StunImmune => ConditionEffectIndex.Stunned,
            _ => ConditionEffectIndex.None
        };
    }

    public static ConditionEffectIndex GetConditionEffectFromName(string name) {
        return Enum.Parse<ConditionEffectIndex>(name.Replace(" ", ""));
    }
}

public enum ConditionEffectIndex : byte
{
    None = 0,
    Dead = 1,
    Quiet = 2,
    Weak = 3,
    Slowed = 4,
    Sick = 5,
    Dazed = 6,
    Stunned = 7,
    Blind = 8,
    Hallucinating = 9,
    Drunk = 10,
    Confused = 11,
    StunImmune = 12,
    Invisible = 13,
    Paralyzed = 14,
    Speedy = 15,
    Bleeding = 16,
    ArmorBrokenImmune = 17,
    Healing = 18,
    Damaging = 19,
    Berserk = 20,
    Paused = 21,
    Stasis = 22,
    StasisImmune = 23,
    Invincible = 24,
    Invulnerable = 25,
    Armored = 26,
    ArmorBroken = 27,
    Hexed = 28,
    NinjaSpeedy = 29,
    Unstable = 30,
    Darkness = 31,
    SlowedImmune = 32,
    DazedImmune = 33,
    ParalyzedImmune = 34,
    Petrify = 35,
    PetrifiedImmune = 36,
    PetEffectIcon = 37,
    Curse = 38,
    CurseImmune = 39,
    HpBoost = 40,
    MpBoost = 41,
    AttBoost = 42,
    DefBoost = 43,
    SpdBoost = 44,
    VitBoost = 45,
    WisBoost = 46,
    DexBoost = 47,
    Silenced = 48,
    Exposed = 49,
    Energized = 50,
    HpDebuff = 51,
    MpDebuff = 52,
    AttDebuff = 53,
    DefDebuff = 54,
    SpdDebuff = 55,
    VitDebuff = 56,
    WisDebuff = 57,
    DexDebuff = 58,
    Inspired = 59,
    ManaDeplete = 60,
    SheatheStance = 61,

    ConditionCount
}

[Flags]
public enum ConditionEffects : ulong
{
    None = 1 << 0,
    Dead = 1 << 1,
    Quiet = 1 << 2,
    Weak = 1 << 3,
    Slowed = 1 << 4,
    Sick = 1 << 5,
    Dazed = 1 << 6,
    Stunned = 1 << 7,
    Blind = 1 << 8,
    Hallucinating = 1 << 9,
    Drunk = 1 << 10,
    Confused = 1 << 11,
    StunImmune = 1 << 12,
    Invisible = 1 << 13,
    Paralyzed = 1 << 14,
    Speedy = 1 << 15,
    Bleeding = 1 << 16,
    ArmorBrokenImmune = 1 << 17,
    Healing = 1 << 18,
    Damaging = 1 << 19,
    Berserk = 1 << 20,
    Paused = 1 << 21,
    Stasis = 1 << 22,
    StasisImmune = 1 << 23,
    Invincible = 1 << 24,
    Invulnerable = 1 << 25,
    Armored = 1 << 26,
    ArmorBroken = 1 << 27,
    Hexed = 1 << 28,
    NinjaSpeedy = 1 << 29,
    Unstable = 1 << 30,
    Darkness = (ulong)1 << 31,
    SlowedImmune =  (ulong)1 << 32,
    DazedImmune =  (ulong)1 << 33,
    ParalyzedImmune =  (ulong)1 << 34,
    Petrify =  (ulong)1 << 35,
    PetrifiedImmune =  (ulong)1 << 36,
    PetEffectIcon =  (ulong)1 << 37,
    Curse =  (ulong)1 << 38,
    CurseImmune =  (ulong)1 << 39,
    HpBoost =  (ulong)1 << 40,
    MpBoost =  (ulong)1 << 41,
    AttBoost =  (ulong)1 << 42,
    DefBoost =  (ulong)1 << 43,
    SpdBoost =  (ulong)1 << 44,
    VitBoost =  (ulong)1 << 45,
    WisBoost =  (ulong)1 << 46,
    DexBoost =  (ulong)1 << 47,
    Silenced =  (ulong)1 << 48,
    Exposed =  (ulong)1 << 49,
    Energized =  (ulong)1 << 50,
    HpDebuff =  (ulong)1 << 51,
    MpDebuff =  (ulong)1 << 52,
    AttDebuff =  (ulong)1 << 53,
    DefDebuff =  (ulong)1 << 54,
    SpdDebuff =  (ulong)1 << 55,
    VitDebuff =  (ulong)1 << 56,
    WisDebuff =  (ulong)1 << 57,
    DexDebuff =  (ulong)1 << 58,
    Inspired =  (ulong)1 << 59,
    ManaDeplete =  (ulong)1 << 60,
    SheatheStance =  (ulong)1 << 61,
}