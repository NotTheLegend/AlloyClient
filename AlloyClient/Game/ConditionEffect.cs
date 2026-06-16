using System;
using System.Collections.Generic;
using System.Linq;
using AlloyClient.Logging;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using BucketType = int; // Backing type for storing bits for incoming statdata, could be a long if server is changed to support that
using EffectType = int; // Backing type of 'ConditionEffect'

namespace AlloyClient.Game;

public unsafe struct ConditionEffectBucket {
    
    private fixed BucketType _buckets[ConditionEffects.MaxEffectBuckets];

    public int TotalIcons { get; private set; }

    public readonly void GetEffectsData(Span<Vector4> span, int index) {
        var idx = 0;
        for (var i = 0; i < ConditionEffects.MaxEffectBuckets; i++) {
            var bits = (uint) (_buckets[i] & ~ConditionEffects.IconlessEffects[i]);

            if (bits == 0) {
                continue;
            }

            for (var b = 0; b < ConditionEffects.MaxBucketSize; b++) {
                if ((bits & (1 << b)) == 0) {
                    continue;
                }
                
                var eff = (ConditionEffect)(b + i * ConditionEffects.MaxBucketSize);
                var icons = ConditionEffects.EffectIcons[eff];
                span[idx] = icons[index % icons.Length];
                idx++;
            }
        }
    }

    public readonly bool HasConditionEffect(ConditionEffect effect) => (_buckets[(EffectType) effect / ConditionEffects.MaxBucketSize] & (1 << ((EffectType) effect % ConditionEffects.MaxBucketSize))) != 0;

    public void SetBucket(int bucketId, int bucketValue) {
        _buckets[bucketId] = bucketValue;
        
        var count = 0;

        for (var i = 0; i < ConditionEffects.MaxEffectBuckets; i++) {
            count += System.Numerics.BitOperations.PopCount((uint)(_buckets[i] & ~ConditionEffects.IconlessEffects[i]));
        }

        TotalIcons =  count;
    }
}

public static class ConditionEffects {

    public const int MaxBucketSize = sizeof(BucketType) * 8;

    public const int MaxEffectBuckets = ((BucketType) ConditionEffect.ConditionEffectCount + MaxBucketSize - 1) / MaxBucketSize;

    private record struct ConditionEffectData(string Name, ConditionEffect Index, int[] IconLookup);

    private static readonly ILogger Logger = ILogger.CreateLogger(nameof(ConditionEffects));

    private static readonly ConditionEffectData[] EffectTable = [
        new("Nothing", ConditionEffect.None, null),
        new("Dead", ConditionEffect.Dead, [0]),
        new("Quiet", ConditionEffect.Quiet, [32]),
        new("Weak", ConditionEffect.Weak, [34, 35, 36, 37]),
        new("Slowed", ConditionEffect.Slowed, [1]),
        new("Sick", ConditionEffect.Sick, [39]),
        new("Dazed", ConditionEffect.Dazed, [44]),
        new("Stunned", ConditionEffect.Stunned, [45]),
        new("Blind", ConditionEffect.Blind, [41]),
        new("Hallucinating", ConditionEffect.Hallucinating, [42]),
        new("Drunk", ConditionEffect.Drunk, [43]),
        new("Confused", ConditionEffect.Confused, [2]),
        new("Stun Immune", ConditionEffect.StunImmune, null),
        new("Invisible", ConditionEffect.Invisible, null),
        new("Paralyzed", ConditionEffect.Paralyzed, [53, 54]),
        new("Speedy", ConditionEffect.Speedy, [0]),
        new("Bleeding", ConditionEffect.Bleeding, [46]),
        new("Healing", ConditionEffect.Healing, [47]),
        new("Damaging", ConditionEffect.Damaging, [49]),
        new("Berserk", ConditionEffect.Berserk, [50]),
        new("Paused", ConditionEffect.Paused, null),
        new("Stasis", ConditionEffect.Stasis, null),
        new("Stasis Immune", ConditionEffect.StasisImmune, null),
        new("Invincible", ConditionEffect.Invincible, null),
        new("Invulnerable", ConditionEffect.Invulnerable, [17]),
        new("Armored", ConditionEffect.Armored, [16]),
        new("Armor Broken", ConditionEffect.ArmorBroken, [55]),
        new("Hexed", ConditionEffect.Hexed, [42]),
        new("Ninja Speedy", ConditionEffect.NinjaSpeedy, [0])
    ];

    public static Span<BucketType> IconlessEffects => new BucketType[MaxEffectBuckets];

    public static readonly Dictionary<ConditionEffect, Vector4[]> EffectIcons = [];

    private static readonly Dictionary<string, ConditionEffect> NameToEffect = [];

    public static void Init() {
        foreach (var effect in EffectTable) {
            if (effect.IconLookup != null) {
                EffectIcons[effect.Index] = effect.IconLookup.Select(i => Main.Atlas.GetAtlasData("lofiInterface2", i).ToVector4()).ToArray();
            } else {
                IconlessEffects[(EffectType) effect.Index / MaxBucketSize] |= (1 << ((EffectType) effect.Index % MaxBucketSize));
            }

            NameToEffect[effect.Name] = effect.Index;
        }
    }

    extension(ConditionEffect) {
        public static ConditionEffect GetImmuneEffect(ConditionEffect effect) {
            return effect switch {
                ConditionEffect.StasisImmune => ConditionEffect.Stasis,
                ConditionEffect.StunImmune => ConditionEffect.Stunned,
                _ => ConditionEffect.None
            };
        }
        
        public static bool IsNegativeCondition(ConditionEffect effect) {
            return effect switch {
                ConditionEffect.Quiet => true,
                ConditionEffect.Weak => true,
                ConditionEffect.Slowed => true,
                ConditionEffect.Sick => true,
                ConditionEffect.Dazed => true,
                ConditionEffect.Stunned => true,
                ConditionEffect.Blind => true,
                ConditionEffect.Hallucinating => true,
                ConditionEffect.Drunk => true,
                ConditionEffect.Confused => true,
                ConditionEffect.Paralyzed => true,
                ConditionEffect.Bleeding => true,
                ConditionEffect.Stasis => true,
                ConditionEffect.ArmorBroken => true,
                ConditionEffect.Hexed => true,
                _ => false
            };
        }
        
        public static void ValueFromName(string name, out EffectType effect) => effect = ConditionEffect.ValueFromName(name);

        public static EffectType ValueFromName(string name) => (EffectType)ConditionEffect.FromNameInternal(name);
        
        public static void FromName(string name, out ConditionEffect effect) => effect = ConditionEffect.FromName(name);

        public static ConditionEffect FromName(string name) => ConditionEffect.FromNameInternal(name);

        private static ConditionEffect FromNameInternal(string name) {
            if (NameToEffect.TryGetValue(name, out var eff)) {
                return eff;
            }

            Logger.Log(LogLevel.Warning, $"Unable to find effect: {name}");
            return ConditionEffect.None;
        }
    }
}

public enum ConditionEffect : EffectType {
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

    ConditionEffectCount
}