using System;
using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class ActivateEffectDesc : ItemData {
    public string EffectName;
    public int EffectId;
    public string ConditionEffect;
    public int CheckExistingEffect;
    public int TotalDamage;
    public double Radius;
    public double EffectDuration;
    public double DurationSec;
    public int DurationMS;
    public int Amount;
    public double Range;
    public double MaximumDistance;
    public string ObjectId;
    public string Id;
    public int MaxTargets;
    public uint Color;
    public int Stats;
    public double Cooldown;
    public bool RemoveSelf;
    public string DungeonName;
    public string LockedName;
    public string Type;
    public bool UseWisMod;
    public string NoWismod;
    public string Target;
    public string Center;
    public int VisualEffect;
    public int AirDurationMS;
    public int SkinType;
    public int ImpactDmg;
    public int NodeReq;
    public int DosesReq;
    public string CurrencyName;
    public int Currency;
    public int HealAmount;
    public double AngleOffset;
    public int StatMod;
    public int StatPerTarget;
    public double DamagePerStat;
    public double AmountPerStat;
    public int StatPerAmount;
    public double RangePerStat;
    public double DurationPerStat;
    public double RadiusPerStat;
    public double CondDurationPerStat;
    public double HealAmountPerStat;
    public double Chance;
    public int MaxAmount;
    public int MaxWisMod;
    public int Tier;
    public bool NoStack;
    public bool OnRelease;
    public string StackEffect;
    public bool FlaskDuration;
    public bool Percent;

    public ActivateEffectDesc(XElement xml) {
        EffectName = xml.Value;
        EffectId = xml.GetValue<int>("Effect");
        ConditionEffect = xml.GetAttribute<string>("effect");
        if (string.IsNullOrWhiteSpace(ConditionEffect)) {
            ConditionEffect = xml.GetAttribute<string>("condEffect");
        }
        if (!string.IsNullOrWhiteSpace(ConditionEffect)) {
            ConditionEffect = GetConditionName(ConditionEffect);
        }
        CheckExistingEffect = xml.GetAttribute<int>("checkExistingEffect");
        TotalDamage = xml.GetAttribute<int>("totalDamage");
        Radius = xml.GetAttribute<double>("radius");
        EffectDuration = xml.GetAttribute<double>("condDuration");
        DurationSec = xml.GetAttribute<double>("duration");
        DurationMS = (int)(DurationSec * 1000);
        Amount = xml.GetAttribute<int>("amount");
        Range = xml.GetAttribute<double>("range");
        MaximumDistance = xml.GetAttribute<double>("maxDistance");
        ObjectId = xml.GetAttribute<string>("objectId");
        Id = xml.GetAttribute<string>("id");
        MaxTargets = xml.GetAttribute<int>("maxTargets");
        Color = xml.GetAttribute<uint>("color");
        Stats = xml.GetAttribute<int>("stat");
        Cooldown = xml.GetAttribute<double>("cooldown");
        RemoveSelf = xml.GetAttribute<bool>("removeSelf");
        DungeonName = xml.GetAttribute<string>("dungeonName");
        LockedName = xml.GetAttribute<string>("lockedName");
        Type = xml.GetAttribute<string>("type");
        UseWisMod = xml.GetAttribute<bool>("useWisMod");
        NoWismod = xml.GetAttribute<string>("noWismod");
        Target = xml.GetAttribute<string>("target");
        Center = xml.GetAttribute<string>("center");
        VisualEffect = xml.GetAttribute<int>("visualEffect");
        AirDurationMS = xml.GetAttribute<int>("airDurationMS");
        SkinType = xml.GetAttribute<int>("skinType");
        ImpactDmg = xml.GetAttribute<int>("impactDmg");
        NodeReq = xml.GetAttribute<int>("nodeReq");
        DosesReq = xml.GetAttribute<int>("dosesReq");
        CurrencyName = xml.GetAttribute<string>("currency");
        // Currency = xml.GetAttribute<int>("Currency"); No XML tag?
        HealAmount = xml.GetAttribute<int>("heal");
        AngleOffset = xml.GetAttribute<double>("angleOffset");
        StatMod = xml.GetAttribute<int>("statMod");
        StatPerTarget = xml.GetAttribute<int>("statPerTarget");
        DamagePerStat = xml.GetAttribute<double>("damagePerStat");
        AmountPerStat = xml.GetAttribute<double>("amountPerStat");
        StatPerAmount = xml.GetAttribute<int>("statPerAmount");
        RangePerStat = xml.GetAttribute<double>("rangePerStat");
        DurationPerStat = xml.GetAttribute<double>("durationPerStat");
        RadiusPerStat = xml.GetAttribute<double>("radiusPerStat");
        CondDurationPerStat = xml.GetAttribute<double>("condDurationPerStat");
        HealAmountPerStat = xml.GetAttribute<double>("healAmountPerStat");
        Chance = xml.GetAttribute<double>("chance");
        MaxAmount = xml.GetAttribute<int>("maxAmount");
        MaxWisMod = xml.GetAttribute<int>("maxWisMod");
        Tier = xml.GetAttribute<int>("tier");
        NoStack = xml.GetAttribute<bool>("noStack");
        OnRelease = xml.GetAttribute<bool>("onRelease");
        StackEffect = xml.GetAttribute<string>("stackEffect");
        if (!string.IsNullOrWhiteSpace(StackEffect)) {
            StackEffect = GetStackName(StackEffect);
        }
        FlaskDuration = xml.GetAttribute<bool>("flaskDuration");
        Percent = xml.GetAttribute<bool>("percent");
        
        AddDefaultNoWisMod();
    }

    private void AddDefaultNoWisMod() {
        this.NoWismod += this.EffectName switch {
            ActivationType.VAMPIRE_BLAST => "healAmount",
            ActivationType.DECOY => "duration",
            ActivationType.TRAP => "condDuration",
            ActivationType.LIGHTNING => "condDuration",
            ActivationType.BULLET_NOVA => "amount",
            ActivationType.DAMAGE_BLAST => "radius duration",
            ActivationType.REMOVE_NEG_COND => "range",
            ActivationType.SPELL_GRENADE => "amount",
            ActivationType.POISON_GRENADE => "radius duration condDuration",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public static string GetConditionName(string eff) {
        return eff switch {
            "EvenMorePowerfulBleeding" => "Bleeding III",
            "PowerfulBleeding" => "Bleeding II",
            "ArmorBroken" => "Armor Broken",
            "EnhancedElixir" => "Enhanced Elixir",
            "ManaSapping" => "Mana Sapping",
            "LichArmy" => "Lich's Army",
            "HeavyStrikes" => "Heavy Strikes",
            "WitchMagic" => "Witch Magic",
            "BloodOath" => "Blood Oath",
            "AngelsFlight" => "Angel's Flight",
            "ManaRush" => "Mana Rush",
            "HolyRestoration" => "Holy Restoration",
            "HealingWounds" => "Healing Wounds",
            "ObsidianPlating" => "Obsidian Plating",
            _ => eff
        };
    }

    public static string GetStackName(string eff) {
        if (eff.Contains("Stack")) {
            eff = eff.Replace("Stack", "");
        }
        return eff;
    }
}