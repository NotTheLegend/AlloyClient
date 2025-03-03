using System;
using System.Collections.Generic;
using System.Linq;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Utils;

namespace MonoClient.Objects.Util;

public sealed class ConditionEffect(string name, ConditionEffects bit, int[] iconOffsets) {
    public readonly string Name = name;
    public readonly ConditionEffects Bit = bit;
    public readonly int[] IconOffsets = iconOffsets;
}

public static class ConditionEffectUtil {
    
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
        new("Not Used", ConditionEffects.NotUsed, null),
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
                EffectIcons[effect.Bit] = effect.IconOffsets.Select(i => Main.Atlas.GetAtlasData("lofiInterface2", i).ToVector4(true)).ToArray();
            else
                IconlessEffects |= effect.Bit;
            
            NameToEffect[effect.Name] = effect.Bit;
        }
    }

    public static int GetConditionEffectId(string name) {
        if (NameToEffect.TryGetValue(name, out var eff))
            return (int)eff;
        
        Logger.Warn($"[ConditionEffect] Unable to find effect: {name}");
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
}

public enum ConditionEffectIndex
{
    None = 0,
    Quiet = 1,
    Weak = 2,
    Slowed = 3,
    Sick = 4,
    Dazed = 5,
    Stunned = 6,
    Blind = 7,
    Hallucinating = 8,
    Drunk = 9,
    Confused = 10,
    StunImmune = 11,
    Invisible = 12,
    Paralyzed = 13,
    Speedy = 14,
    Bleeding = 15,
    NotUsed = 16,
    Healing = 17,
    Damaging = 18,
    Berserk = 19,
    Paused = 20,
    Stasis = 21,
    StasisImmune = 22,
    Invincible = 23,
    Invulnerable = 24,
    Armored = 25,
    ArmorBroken = 26,
    Hexed = 27,
    NinjaSpeedy = 28,
}

[Flags]
public enum ConditionEffects : ulong
{
    None = 1 << 0,
    Quiet = 1 << 1,
    Weak = 1 << 2,
    Slowed = 1 << 3,
    Sick = 1 << 4,
    Dazed = 1 << 5,
    Stunned = 1 << 6,
    Blind = 1 << 7,
    Hallucinating = 1 << 8,
    Drunk = 1 << 9,
    Confused = 1 << 10,
    StunImmune = 1 << 11,
    Invisible = 1 << 12,
    Paralyzed = 1 << 13,
    Speedy = 1 << 14,
    Bleeding = 1 << 15,
    NotUsed = 1 << 16,
    Healing = 1 << 17,
    Damaging = 1 << 18,
    Berserk = 1 << 19,
    Paused = 1 << 20,
    Stasis = 1 << 21,
    StasisImmune = 1 << 22,
    Invincible = 1 << 23,
    Invulnerable = 1 << 24,
    Armored = 1 << 25,
    ArmorBroken = 1 << 26,
    Hexed = 1 << 27,
    NinjaSpeedy = 1 << 28
}