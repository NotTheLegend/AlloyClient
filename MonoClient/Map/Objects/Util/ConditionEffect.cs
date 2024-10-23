using System.Collections.Generic;

namespace MonoClient.Objects.Util;

public class ConditionEffect(string name, int bit, int[] iconOffsets, bool icon16Bit = false, bool showAboveEntity = true) {
    public string Name = name;
    public int Bit = bit;
    public int[] IconOffsets = iconOffsets;
    public bool Icon16Bit = icon16Bit;
    public bool ShowAboveEntity = showAboveEntity;
}

public static class ConditionEffectUtil {
    
    public const int DEAD = 0;
    public const int QUIET = 1;
    public const int WEAK = 2;
    public const int SLOWED = 3;
    public const int SICK = 4;
    public const int DAZED = 5;
    public const int STUNNED = 6;
    public const int BLIND = 7;
    public const int HALLUCINATING = 8;
    public const int DRUNK = 9;
    public const int CONFUSED = 10;
    public const int STUN_IMMUNE = 11;
    public const int INVISIBLE = 12;
    public const int PARALYZED = 13;
    public const int SPEEDY = 14;
    public const int BLEEDING = 15;
    public const int HIDDEN = 16;
    public const int HEALING = 17;
    public const int DAMAGING = 18;
    public const int BERSERK = 19;
    public const int PAUSED = 20;
    public const int STASIS = 21;
    public const int STASIS_IMMUNE = 22;
    public const int INVINCIBLE = 23;
    public const int INVULNERABLE = 24;
    public const int ARMORED = 25;
    public const int ARMOR_BROKEN = 26;
    public const int HEXED = 27;
    public const int NINJA_SPEEDY = 28;
    public const int RAGE = 29;
    public const int CURSE = 30;
    public const int UNSTABLE = 31;
    public const int DARKNESS = 32;
    public const int PARALYZE_IMMUNE = 33;
    public const int DAZED_IMMUNE = 34;
    public const int SLOWED_IMMUNE = 35;
    public const int UNUSED_2 = 36;
    public const int PIERCING_IMMUNE = 37;
    public const int ARMORED_IMMUNE = 38;
    public const int SICK_IMMUNE = 39;
    public const int DEATHMARK = 40;
    public const int FERAL = 41;
    public const int SILENCED = 42;
    public const int HP_BOOST = 43;
    public const int MP_BOOST = 44;
    public const int ATT_BOOST = 45;
    public const int DEF_BOOST = 46;
    public const int SPD_BOOST = 47;
    public const int DEX_BOOST = 48;
    public const int VIT_BOOST = 49;
    public const int WIS_BOOST = 50;
    public const int NEGATIVE_HP_BOOST = 51;
    public const int NEGATIVE_MP_BOOST = 52;
    public const int NEGATIVE_ATT_BOOST = 53;
    public const int NEGATIVE_DEF_BOOST = 54;
    public const int NEGATIVE_SPD_BOOST = 55;
    public const int NEGATIVE_DEX_BOOST = 56;
    public const int NEGATIVE_VIT_BOOST = 57;
    public const int NEGATIVE_WIS_BOOST = 58;
    public const int POWERFUL_BLEEDING = 59;
    public const int EVEN_MORE_POWERFUL_BLEEDING = 60;
    public const int KINGS_MADNESS = 61;
    public const int MIDAS_TOUCH = 62;
    public const int SCORCHED = 63;
    public const int DRAIN = 64;
    public const int RUIN = 65;
    public const int CRIPPLED = 66;
    public const int MADNESS_IMMUNE = 67;
    public const int BUNNY_DAMAGING = 68;
    public const int BUNNY_MANA_REGEN = 69;
    public const int BUNNY_HEALING = 70;
    public const int BUNNY_SPEEDY = 71;
    public const int BUNNY_WEAK = 72;
    public const int BUNNY_CURSE = 73;
    public const int INCINERATION_BLEED = 74;
    public const int MONKEY_CURSE = 75;
    public const int DRAKENGUARD_SCORCHED = 76;
    public const int PITCH_BLACK = 77;
    public const int PERFECT_DARK = 78;
    public const int MANA_RESTORATION = 79;
    public const int GLOOPED = 80;
    public const int ULTRASICK = 81;
    public const int BURNING = 82;
    public const int REFRESHED = 83;
    public const int FRAGILE = 84;
    public const int REAPING = 85;
    public const int CORROSIVE = 86;
    public const int ENHANCED_ELIXIR = 87;
    public const int MANA_SAPPING = 88;
    public const int GUARDED = 89;
    public const int RECHARGED = 90;
    public const int LUCKY = 91;
    public const int ALMIGHTY = 92;
    public const int LICH_ARMY = 93;
    public const int HEAVY_STRIKES = 94;
    public const int WITCH_MAGIC = 95;
    public const int PRECISE = 96;
    public const int VENOMOUS = 97;
    public const int BLOOD_OATH = 98;
    public const int CHAOSBURN = 99;
    public const int GRACE = 100;
    public const int CLOUDWALKER = 101;
    public const int ANGELS_FLIGHT = 102;
    public const int MANA_RUSH = 103;
    public const int STUDYING = 104;
    public const int HOLY_RESTORATION = 105;
    public const int PARRYING = 106;
    public const int HEALING_WOUNDS = 107;
    public const int OBSIDIAN_PLATING = 108;
    public const int SHEEPIFIED = 109;
    
    public const int DEAD_BIT = 1 << DEAD;
    public const int QUIET_BIT = 1 << QUIET;
    public const int WEAK_BIT = 1 << WEAK;
    public const int SLOWED_BIT = 1 << SLOWED;
    public const int SICK_BIT = 1 << SICK;
    public const int DAZED_BIT = 1 << DAZED;
    public const int STUNNED_BIT = 1 << STUNNED;
    public const int BLIND_BIT = 1 << BLIND;
    public const int HALLUCINATING_BIT = 1 << HALLUCINATING;
    public const int DRUNK_BIT = 1 << DRUNK;
    public const int CONFUSED_BIT = 1 << CONFUSED;
    public const int STUN_IMMUNE_BIT = 1 << STUN_IMMUNE;
    public const int INVISIBLE_BIT = 1 << INVISIBLE;
    public const int PARALYZED_BIT = 1 << PARALYZED;
    public const int SPEEDY_BIT = 1 << SPEEDY;
    public const int BLEEDING_BIT = 1 << BLEEDING;
    public const int HIDDEN_BIT = 1 << HIDDEN;
    public const int HEALING_BIT = 1 << HEALING;
    public const int DAMAGING_BIT = 1 << DAMAGING;
    public const int BERSERK_BIT = 1 << BERSERK;
    public const int PAUSED_BIT = 1 << PAUSED;
    public const int STASIS_BIT = 1 << STASIS;
    public const int STASIS_IMMUNE_BIT = 1 << STASIS_IMMUNE;
    public const int INVINCIBLE_BIT = 1 << INVINCIBLE;
    public const int INVULNERABLE_BIT = 1 << INVULNERABLE;
    public const int ARMORED_BIT = 1 << ARMORED;
    public const int ARMOR_BROKEN_BIT = 1 << ARMOR_BROKEN;
    public const int HEXED_BIT = 1 << HEXED;
    public const int NINJA_SPEEDY_BIT = 1 << NINJA_SPEEDY;
    public const int RAGE_BIT = 1 << RAGE;
    public const int CURSE_BIT = 1 << CURSE;
    public const int UNSTABLE_BIT = 1 << UNSTABLE;
    public const int DARKNESS_BIT = 1 << DARKNESS;
    public const int PARALYZE_IMMUNE_BIT = 1 << (PARALYZE_IMMUNE - SECOND_BATCH_THRESHOLD);
    public const int DAZED_IMMUNE_BIT = 1 << (DAZED_IMMUNE - SECOND_BATCH_THRESHOLD);
    public const int SLOWED_IMMUNE_BIT = 1 << (SLOWED_IMMUNE - SECOND_BATCH_THRESHOLD);
    public const int UNUSED_2_BIT = 1 << (UNUSED_2 - SECOND_BATCH_THRESHOLD);
    public const int PIERCING_IMMUNE_BIT = 1 << (PIERCING_IMMUNE - SECOND_BATCH_THRESHOLD);
    public const int ARMORED_IMMUNE_BIT = 1 << (ARMORED_IMMUNE - SECOND_BATCH_THRESHOLD);
    public const int SICK_IMMUNE_BIT = 1 << (SICK_IMMUNE - SECOND_BATCH_THRESHOLD);
    public const int DEATHMARK_BIT = 1 << (DEATHMARK - SECOND_BATCH_THRESHOLD);
    public const int FERAL_BIT = 1 << (FERAL - SECOND_BATCH_THRESHOLD);
    public const int SILENCED_BIT = 1 << (SILENCED - SECOND_BATCH_THRESHOLD);
    public const int HP_BOOST_BIT = 1 << (HP_BOOST - SECOND_BATCH_THRESHOLD);
    public const int MP_BOOST_BIT = 1 << (MP_BOOST - SECOND_BATCH_THRESHOLD);
    public const int ATT_BOOST_BIT = 1 << (ATT_BOOST - SECOND_BATCH_THRESHOLD);
    public const int DEF_BOOST_BIT = 1 << (DEF_BOOST - SECOND_BATCH_THRESHOLD);
    public const int SPD_BOOST_BIT = 1 << (SPD_BOOST - SECOND_BATCH_THRESHOLD);
    public const int DEX_BOOST_BIT = 1 << (DEX_BOOST - SECOND_BATCH_THRESHOLD);
    public const int VIT_BOOST_BIT = 1 << (VIT_BOOST - SECOND_BATCH_THRESHOLD);
    public const int WIS_BOOST_BIT = 1 << (WIS_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_HP_BOOST_BIT = 1 << (NEGATIVE_HP_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_MP_BOOST_BIT = 1 << (NEGATIVE_MP_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_ATT_BOOST_BIT = 1 << (NEGATIVE_ATT_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_DEF_BOOST_BIT = 1 << (NEGATIVE_DEF_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_SPD_BOOST_BIT = 1 << (NEGATIVE_SPD_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_DEX_BOOST_BIT = 1 << (NEGATIVE_DEX_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_VIT_BOOST_BIT = 1 << (NEGATIVE_VIT_BOOST - SECOND_BATCH_THRESHOLD);
    public const int NEGATIVE_WIS_BOOST_BIT = 1 << (NEGATIVE_WIS_BOOST - SECOND_BATCH_THRESHOLD);
    public const int POWERFUL_BLEEDING_BIT = 1 << (POWERFUL_BLEEDING - SECOND_BATCH_THRESHOLD);
    public const int EVEN_MORE_POWERFUL_BLEEDING_BIT = 1 << (EVEN_MORE_POWERFUL_BLEEDING - SECOND_BATCH_THRESHOLD);
    public const int KINGS_MADNESS_BIT = 1 << (KINGS_MADNESS - SECOND_BATCH_THRESHOLD);
    public const int MIDAS_TOUCH_BIT = 1 << (MIDAS_TOUCH - SECOND_BATCH_THRESHOLD);
    public const int SCORCHED_BIT = 1 << (SCORCHED - SECOND_BATCH_THRESHOLD);
    public const int DRAIN_BIT = 1 << (DRAIN - THIRD_BATCH_THRESHOLD);
    public const int RUIN_BIT = 1 << (RUIN - THIRD_BATCH_THRESHOLD);
    public const int CRIPPLED_BIT = 1 << (CRIPPLED - THIRD_BATCH_THRESHOLD);
    public const int MADNESS_IMMUNE_BIT = 1 << (MADNESS_IMMUNE - THIRD_BATCH_THRESHOLD);
    public const int BUNNY_DAMAGING_BIT = 1 << (BUNNY_DAMAGING - THIRD_BATCH_THRESHOLD);
    public const int BUNNY_MANA_REGEN_BIT = 1 << (BUNNY_MANA_REGEN - THIRD_BATCH_THRESHOLD);
    public const int BUNNY_HEALING_BIT = 1 << (BUNNY_HEALING - THIRD_BATCH_THRESHOLD);
    public const int BUNNY_SPEEDY_BIT = 1 << (BUNNY_SPEEDY - THIRD_BATCH_THRESHOLD);
    public const int BUNNY_WEAK_BIT = 1 << (BUNNY_WEAK - THIRD_BATCH_THRESHOLD);
    public const int BUNNY_CURSE_BIT = 1 << (BUNNY_CURSE - THIRD_BATCH_THRESHOLD);
    public const int INCINERATION_BLEED_BIT = 1 << (INCINERATION_BLEED - THIRD_BATCH_THRESHOLD);
    public const int MONKEY_CURSE_BIT = 1 << (MONKEY_CURSE - THIRD_BATCH_THRESHOLD);
    public const int DRAKENGUARD_SCORCHED_BIT = 1 << (DRAKENGUARD_SCORCHED - THIRD_BATCH_THRESHOLD);
    public const int PITCH_BLACK_BIT = 1 << (PITCH_BLACK - THIRD_BATCH_THRESHOLD);
    public const int PERFECT_DARK_BIT = 1 << (PERFECT_DARK - THIRD_BATCH_THRESHOLD);
    public const int MANA_RESTORATION_BIT = 1 << (MANA_RESTORATION - THIRD_BATCH_THRESHOLD);
    public const int GLOOPED_BIT = 1 << (GLOOPED - THIRD_BATCH_THRESHOLD);
    public const int ULTRASICK_BIT = 1 << (ULTRASICK - THIRD_BATCH_THRESHOLD);
    public const int BURNING_BIT = 1 << (BURNING - THIRD_BATCH_THRESHOLD);
    public const int REFRESHED_BIT = 1 << (REFRESHED - THIRD_BATCH_THRESHOLD);
    public const int FRAGILE_BIT = 1 << (FRAGILE - THIRD_BATCH_THRESHOLD);
    public const int REAPING_BIT = 1 << (REAPING - THIRD_BATCH_THRESHOLD);
    public const int CORROSIVE_BIT = 1 << (CORROSIVE - THIRD_BATCH_THRESHOLD);
    public const int ENHANCED_ELIXIR_BIT = 1 << (ENHANCED_ELIXIR - THIRD_BATCH_THRESHOLD);
    public const int MANA_SAPPING_BIT = 1 << (MANA_SAPPING - THIRD_BATCH_THRESHOLD);
    public const int GUARDED_BIT = 1 << (GUARDED - THIRD_BATCH_THRESHOLD);
    public const int RECHARGED_BIT = 1 << (RECHARGED - THIRD_BATCH_THRESHOLD);
    public const int LUCKY_BIT = 1 << (LUCKY - THIRD_BATCH_THRESHOLD);
    public const int ALMIGHTY_BIT = 1 << (ALMIGHTY - THIRD_BATCH_THRESHOLD);
    public const int LICH_ARMY_BIT = 1 << (LICH_ARMY - THIRD_BATCH_THRESHOLD);
    public const int HEAVY_STRIKES_BIT = 1 << (HEAVY_STRIKES - THIRD_BATCH_THRESHOLD);
    public const int WITCH_MAGIC_BIT = 1 << (WITCH_MAGIC - THIRD_BATCH_THRESHOLD);
    public const int PRECISE_BIT = 1 << (PRECISE - FOURTH_BATCH_THRESHOLD);
    public const int VENOMOUS_BIT = 1 << (VENOMOUS - FOURTH_BATCH_THRESHOLD);
    public const int BLOOD_OATH_BIT = 1 << (BLOOD_OATH - FOURTH_BATCH_THRESHOLD);
    public const int CHAOSBURN_BIT = 1 << (CHAOSBURN - FOURTH_BATCH_THRESHOLD);
    public const int GRACE_BIT = 1 << (GRACE - FOURTH_BATCH_THRESHOLD);
    public const int CLOUDWALKER_BIT = 1 << (CLOUDWALKER - FOURTH_BATCH_THRESHOLD);
    public const int ANGELS_FLIGHT_BIT = 1 << (ANGELS_FLIGHT - FOURTH_BATCH_THRESHOLD);
    public const int MANA_RUSH_BIT = 1 << (MANA_RUSH - FOURTH_BATCH_THRESHOLD);
    public const int STUDYING_BIT = 1 << (STUDYING - FOURTH_BATCH_THRESHOLD);
    public const int HOLY_RESTORATION_BIT = 1 << (HOLY_RESTORATION - FOURTH_BATCH_THRESHOLD);
    public const int PARRYING_BIT = 1 << (PARRYING - FOURTH_BATCH_THRESHOLD);
    public const int HEALING_WOUNDS_BIT = 1 << (HEALING_WOUNDS - FOURTH_BATCH_THRESHOLD);
    public const int OBSIDIAN_PLATING_BIT = 1 << (OBSIDIAN_PLATING - FOURTH_BATCH_THRESHOLD);
    public const int SHEEPIFIED_BIT = 1 << (SHEEPIFIED - FOURTH_BATCH_THRESHOLD);

    public const int FirstBatch = 0;
    public const int SecondBatch = 1;
    public const int ThirdBatch = 2;
    public const int SECOND_BATCH_THRESHOLD = 32;
    public const int THIRD_BATCH_THRESHOLD = 64;
    public const int FOURTH_BATCH_THRESHOLD = 96;

    public const int NumConditionEffectBatches = FirstBatch + SecondBatch + ThirdBatch;

    public static ConditionEffect[] Effects = [
        new("Dead", DEAD_BIT, null),
        new("Quiet", QUIET_BIT, [32]),
        new("Weak", WEAK_BIT, [34, 35, 36, 37]),
        new("Slowed", SLOWED_BIT, [1]),
        new("Sick", SICK_BIT, [39]),
        new("Dazed", DAZED_BIT, [44]),
        new("Stunned", STUNNED_BIT, [45]),
        new("Blind", BLIND_BIT, [41]),
        new("Hallucinating", HALLUCINATING_BIT, [42]),
        new("Drunk", DRUNK_BIT, [43]),
        new("Confused", CONFUSED_BIT, [2]),
        new("Stun Immune", STUN_IMMUNE_BIT, null),
        new("Invisible", INVISIBLE_BIT, [114], false, false),
        new("Paralyzed", PARALYZED_BIT, [53, 54]),
        new("Speedy", SPEEDY_BIT, [0]),
        new("Bleeding", BLEEDING_BIT, [46]),
        new("Hidden", HIDDEN_BIT, null),
        new("Healing", HEALING_BIT, [47]),
        new("Damaging", DAMAGING_BIT, [49]),
        new("Berserk", BERSERK_BIT, [50]),
        new("Paused", PAUSED_BIT, null),
        new("Stasis", STASIS_BIT, null),
        new("Stasis Immune", STASIS_IMMUNE_BIT, null),
        new("Invincible", INVINCIBLE_BIT, null),
        new("Invulnerable", INVULNERABLE_BIT, [17]),
        new("Armored", ARMORED_BIT, [16]),
        new("Armor Broken", ARMOR_BROKEN_BIT, [55]),
        new("Hexed", HEXED_BIT, [42]),
        new("Ninja Speedy", NINJA_SPEEDY_BIT, [0]),
        new("Rage", RAGE_BIT, null),
        new("Curse", CURSE_BIT, [0x38]),
        new("Unstable", UNSTABLE_BIT, [0x39]),
        new("Darkness", DARKNESS_BIT, null),
        new("Paralyze Immune", PARALYZE_IMMUNE_BIT, null),
        new("Dazed Immune", DAZED_IMMUNE_BIT, null),
        new("Slowed Immune", SLOWED_IMMUNE_BIT, null),
        new("Unused 2", UNUSED_2_BIT, null),
        new("Piercing Immune", PIERCING_IMMUNE_BIT, null),
        new("Armored Immune", ARMORED_IMMUNE_BIT, null),
        new("Sick Immune", SICK_IMMUNE_BIT, null),
        new("Death Mark", DEATHMARK_BIT, [0x3b]),
        new("Feral", FERAL_BIT, [0x3c]),
        new("Silenced", SILENCED_BIT, [0x3d]),
        new("HP Boost", HP_BOOST_BIT, [48], true),
        new("MP Boost", MP_BOOST_BIT, [49], true),
        new("ATT Boost", ATT_BOOST_BIT, [50], true),
        new("DEF Boost", DEF_BOOST_BIT, [51], true),
        new("SPD Boost", SPD_BOOST_BIT, [52], true),
        new("DEX Boost", DEX_BOOST_BIT, [53], true),
        new("VIT Boost", VIT_BOOST_BIT, [54], true),
        new("WIS Boost", WIS_BOOST_BIT, [55], true),
        new("Negative HP Boost", NEGATIVE_HP_BOOST_BIT, [56], true),
        new("Negative MP Boost", NEGATIVE_MP_BOOST_BIT, [57], true),
        new("Negative ATT Boost", NEGATIVE_ATT_BOOST_BIT, [58], true),
        new("Negative DEF Boost", NEGATIVE_DEF_BOOST_BIT, [59], true),
        new("Negative SPD Boost", NEGATIVE_SPD_BOOST_BIT, [60], true),
        new("Negative DEX Boost", NEGATIVE_DEX_BOOST_BIT, [61], true),
        new("Negative VIT Boost", NEGATIVE_VIT_BOOST_BIT, [62], true),
        new("Negative WIS Boost", NEGATIVE_WIS_BOOST_BIT, [63], true),
        new("Bleeding II", POWERFUL_BLEEDING_BIT, [80]),
        new("Bleeding III", EVEN_MORE_POWERFUL_BLEEDING_BIT, [81]),
        new("King's Madness", KINGS_MADNESS_BIT, [82]),
        new("Midas Touch", MIDAS_TOUCH_BIT, [83]),
        new("Scorched", SCORCHED_BIT, [81]),
        new("Drain", DRAIN_BIT, [84]),
        new("Ruin", RUIN_BIT, [85]),
        new("Crippled", CRIPPLED_BIT, [0x3e]),
        new("Madness Immune", MADNESS_IMMUNE_BIT, null),
        new("Bunny Damaging", BUNNY_DAMAGING_BIT, [86]),
        new("Bunny Mana Regen", BUNNY_MANA_REGEN_BIT, [87]),
        new("Bunny Healing", BUNNY_HEALING_BIT, [88]),
        new("Bunny Speedy", BUNNY_SPEEDY_BIT, [89]),
        new("Bunny Weak", BUNNY_WEAK_BIT, [90]),
        new("Bunny Curse", BUNNY_CURSE_BIT, [91]),
        new("Incineration Bleed", INCINERATION_BLEED_BIT, [81]),
        new("Monkey Curse", MONKEY_CURSE_BIT, [0x3f]),
        new("Scorched", DRAKENGUARD_SCORCHED_BIT, [81]),
        new("Pitch Black", PITCH_BLACK_BIT, [92]),
        new("Perfect Dark", PERFECT_DARK_BIT, [93]),
        new("Mana Restoration", MANA_RESTORATION_BIT, [94]),
        new("Glooped", GLOOPED_BIT, [95]),
        new("Ultrasick", ULTRASICK_BIT, [102]),
        new("Burning", BURNING_BIT, [106]),
        new("Refreshed", REFRESHED_BIT, [107]),
        new("Fragile", FRAGILE_BIT, [116]),
        new("Reaping", REAPING_BIT, [117]),
        new("Corrosive", CORROSIVE_BIT, [118]),
        new("Enhanced Elixir", ENHANCED_ELIXIR_BIT, [119]),
        new("Mana Sapping", MANA_SAPPING_BIT, [120]),
        new("Guarded", GUARDED_BIT, [121]),
        new("Recharged", RECHARGED_BIT, [122]),
        new("Lucky", LUCKY_BIT, [123]),
        new("Almighty", ALMIGHTY_BIT, [124]),
        new("Lich's Army", LICH_ARMY_BIT, [125]),
        new("Heavy Strikes", HEAVY_STRIKES_BIT, [126]),
        new("Witch Magic", WITCH_MAGIC_BIT, [127]),
        new("Precise", PRECISE_BIT, [128]),
        new("Venomous", VENOMOUS_BIT, [129]),
        new("Blood Oath", BLOOD_OATH_BIT, [130]),
        new("Chaosburn", CHAOSBURN_BIT, [131]),
        new("Grace", GRACE_BIT, [133]),
        new("Cloudwalker", CLOUDWALKER_BIT, [134]),
        new("Angel's Flight", ANGELS_FLIGHT_BIT, [135]),
        new("Mana Rush", MANA_RUSH_BIT, [136]),
        new("Studying", STUDYING_BIT, [137]),
        new("Holy Restoration", HOLY_RESTORATION_BIT, [138]),
        new("Parrying", PARRYING_BIT, [139]),
        new("Healing Wounds", HEALING_WOUNDS_BIT, [140]),
        new("Obsidian Plating", OBSIDIAN_PLATING_BIT, [141]),
        new("Sheepified", SHEEPIFIED_BIT, [142])
    ];

    private static Dictionary<string, int> _conditionNameToId;

    public static int GetConditionEffectId(string name) {
        if (_conditionNameToId == null) {
            _conditionNameToId = new();
            foreach (var effect in Effects) {
                _conditionNameToId[effect.Name] = effect.Bit;
            }
        }

        int ret = 0;
        foreach (var kvp in _conditionNameToId) {
            if (kvp.Key == name.Trim()) {
                ret = kvp.Value;
                break;
            }
        }
        return ret;
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
            ConditionEffectIndex.Darkness => true,
            ConditionEffectIndex.Hallucinating => true,
            ConditionEffectIndex.Drunk => true,
            ConditionEffectIndex.Confused => true,
            ConditionEffectIndex.Paralyzed => true,
            ConditionEffectIndex.Bleeding => true,
            ConditionEffectIndex.Stasis => true,
            ConditionEffectIndex.ArmorBroken => true,
            ConditionEffectIndex.Hexed => true,
            ConditionEffectIndex.Curse => true,
            ConditionEffectIndex.Unstable => true,
            ConditionEffectIndex.Silenced => true,
            ConditionEffectIndex.PowerfulBleeding => true,
            ConditionEffectIndex.EvenMorePowerfulBleeding => true,
            ConditionEffectIndex.KingsMadness => true,
            ConditionEffectIndex.Drain => true,
            _ => false
        };
    }

    public static ConditionEffectIndex GetImmuneEffectIndex(ConditionEffectIndex effect) {
        return effect switch {
            ConditionEffectIndex.StasisImmune => ConditionEffectIndex.Stasis,
            ConditionEffectIndex.StunImmune => ConditionEffectIndex.Stunned,
            ConditionEffectIndex.ParalyzeImmune => ConditionEffectIndex.Paralyzed,
            ConditionEffectIndex.DazedImmune => ConditionEffectIndex.Dazed,
            ConditionEffectIndex.MadnessImmune => ConditionEffectIndex.KingsMadness,
            ConditionEffectIndex.ArmoredImmune => ConditionEffectIndex.ArmorBroken,
            ConditionEffectIndex.SickImmune => ConditionEffectIndex.Sick,
            ConditionEffectIndex.SlowedImmune => ConditionEffectIndex.Slowed,
            _ => ConditionEffectIndex.None
        };
    }
}

public enum ConditionEffectIndex : byte {
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
    Hidden = 16,
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
    Rage = 29,
    Curse = 30,
    Unstable = 31,
    Darkness = 32,
    ParalyzeImmune = 33,
    DazedImmune = 34,
    SlowedImmune = 35,
    Warging = 36,
    PiercingImmune = 37,
    ArmoredImmune = 38,
    SickImmune = 39,
    DeathMark = 40,
    Feral = 41,
    Silenced = 42,
    HpBoost = 43,
    MpBoost = 44,
    AttBoost = 45,
    DefBoost = 46,
    SpdBoost = 47,
    DexBoost = 48,
    VitBoost = 49,
    WisBoost = 50,
    NegativeHpBoost = 51,
    NegativeMpBoost = 52,
    NegativeAttBoost = 53,
    NegativeDefBoost = 54,
    NegativeSpdBoost = 55,
    NegativeDexBoost = 56,
    NegativeVitBoost = 57,
    NegativeWisBoost = 58,
    PowerfulBleeding = 59,
    EvenMorePowerfulBleeding = 60,
    KingsMadness = 61,
    MidasTouch = 62,
    Scorched = 63,
    Drain = 64,
    Ruin = 65,
    Crippled = 66,
    MadnessImmune = 67,
    BunnyDamaging = 68,
    BunnyManaRegen = 69,
    BunnyHealing = 70,
    BunnySpeedy = 71,
    BunnyWeak = 72,
    BunnyCurse = 73,
    IncinerationBleed = 74,
    MonkeyCurse = 75,
    DrakenguardScorched = 76,
    PitchBlack = 77,
    PerfectDark = 78,
    ManaRestoration = 79,
    Glooped = 80
}