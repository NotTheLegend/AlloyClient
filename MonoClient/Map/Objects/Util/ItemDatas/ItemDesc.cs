using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Common;
using MonoClient.Assets.XmlStructs;
using MonoClient.Ui.Components.Tooltips;
using Newtonsoft.Json;

namespace MonoClient.Objects.Util.ItemDatas;

public class ItemDesc : ItemData {
    public ushort ObjectType;
    public string ObjectId;
    public string Class;
    public string DisplayId;
    public int Tex1;
    public int Tex2;
    public int SlotType;
    public string Description;
    public bool Consumable;
    public bool InvUse;
    public bool Soulbound;
    public bool AdminGiven;
    public bool Potion;
    public bool Usable;
    public bool Reusable;
    public bool Resurrects;
    public int Tier;
    public int BagType;
    public int FameBonus;
    public int NumProjectiles;
    public int MpDrainCost;
    public int HpCost;
    public int HpDrainCost;
    public int AlternativeHpDrainCost;
    public int Doses;
    public string SuccessorId;
    public bool Backpack;
    public bool LDBoosted;
    public int LDBoostAmount;
    public bool LTBoosted;
    public bool XpBoost;
    public bool SkillXPBoost;
    public double Timer;
    public int MpEndCost;
    public bool Legendary;
    public int HunterLevel;
    public int MaxDoses;
    public double LootBoost;
    public int BoosterId;
    public int UpgradeResult;
    public int HarvestedSouls;
    public int UpgradeCost;
    public int UpgradeRequirement;
    public int Durability;
    public bool Cosmic;
    public bool Null;
    public bool Treasure;
    public bool PermaPet;
    public bool HF0;
    public bool HF1;
    public bool HF2;
    public bool HF3;
    public bool HF4;
    public bool HI0;
    public bool HI1;
    public bool HI2;
    public bool HI3;
    public bool HI4;
    public bool H5;
    public int TabletSlots;
    public int MaxTabletSlots;
    public string OldSound;
    public string Sound;
    public bool Exotic;
    public bool Demonic;
    public bool Angelic;
    public string TransformResult;
    public bool Transformed;
    public bool EpicKey;
    public bool HalloweenLegendary;
    public bool Awakened;
    public int[] AwakenedEffects;
    public bool RandomEffect;
    public bool ChristmasLegendary;
    public bool EasterLegendary;
    public bool Rainbow;
    public bool Held;
    public bool ShowTierTag;
    public bool VaultItem;
    public bool NotMarketable;
    public string ReforgeStone;
    public bool NoEssences;
    public int Essences;
    public int MaxEssences;
    public int LimitedUses;
    public int UsesLeft;
    public bool CraftingMaterial;
    public int LegendarySacrifices;
    public int DemonicSacrifices;
    public bool FusionInteraction;
    public bool Shattered;
    public string RewardFor;
    public bool SnowballerReward;
    public bool EffectsArePassive;
    public string ReskinOf;
    public int DeathBoostAmount;
    public string DropLocation;
    public bool BrewingItem;
    public int AltTextureIndex = 0;
    public bool DungeonKey;
    public int[] DungeonModifiers;
    public bool OnlyUsableInVault;
    public bool RefundOnDeath;
    public string SpecialDragInteraction;
    public int BrewingPoints = 0;
    public int CraftingRarity = 0;
    public ProjectileDesc Projectile;
    public int[] ItemEffect; // Don't rename
    public BrewingEffectDesc BrewingEffects;
    public float _cooldown;
    public int UseCharges;
    public EssenceUpgradeManager EssenceUpgrades = new EssenceUpgradeManager();
    public int MaxCharges;
    public string FlaskType;
    public int CurrentCharges;
    public int Quality;
    public int[] Enchantments;
    public bool Sealed;
    public ActivateEffectDesc[] SealedEffects;
    public StatBoostDesc[] StatBoosts;
    public bool Currency;
    public int[] OverwriteEffects;
    public StatBoostDesc[] InventoryStatBoosts;
    public ActivateEffectDesc[] Activate;
    public ItemAnimation Animation;
    public CustomToolTipData[] CustomToolTipDataList;
    public MaskDesc MaskDesc;

    private float _flaskDuration;

    public float FlaskDuration {
        get => EquipmentToolTip.Round(_flaskDuration + this.Quality / 100.0f * _flaskDuration);
        set => _flaskDuration = value;
    }

    private float _rateOfFire;

    public float RateOfFire {
        get => _rateOfFire;
        set => _rateOfFire = value;
    }

    private float _arcGap;

    public float ArcGap {
        get => _arcGap;
        set => _arcGap = value;
    }

    private float _mpCost;

    public float MpCost {
        get => _mpCost;
        set => _mpCost = value;
    }

    public float Cooldown {
        get => _cooldown;
        set => _cooldown = value;
    }
    
    public ItemDesc(XElement xml) {
        ObjectType = xml.GetAttribute<ushort>("type");
        ObjectId = xml.GetAttribute<string>("id");
        Class = xml.GetValue<string>("Class");
        DisplayId = xml.GetValue<string>("Class");
        Tex1 = xml.GetValue<int>("Tex1");
        Tex2 = xml.GetValue<int>("Tex2");
        SlotType = xml.GetValue<int>("SlotType");
        Description = xml.GetValue<string>("Description");
        Consumable = xml.GetValue<bool>("Consumable");
        InvUse = xml.GetValue<bool>("InvUse");
        Soulbound = xml.GetValue<bool>("Soulbound");
        Potion = xml.GetValue<bool>("Potion");
        Usable = xml.GetValue<bool>("Usable");
        Reusable = xml.GetValue<bool>("Reusable");
        Resurrects = xml.GetValue<bool>("Resurrects");
        Tier = xml.GetValue<int>("Tier", -1);
        BagType = xml.GetValue<int>("BagType");
        FameBonus = xml.GetValue<int>("FameBonus");
        NumProjectiles = xml.GetValue<int>("NumProjectiles");
        _arcGap = xml.GetValue<float>("ArcGap");
        _mpCost = xml.GetValue<float>("MpCost");
        _cooldown = xml.GetValue<float>("Cooldown", 0.5f);
        MpDrainCost = xml.GetValue<int>("MpDrainCost", -1);
        HpCost = xml.GetValue<int>("HpCost");
        HpDrainCost = xml.GetValue<int>("HpDrainCost", -1);
        AlternativeHpDrainCost = xml.GetValue<int>("AlternativeHpDrainCost", -1);
        Doses = xml.GetValue<int>("Doses");
        SuccessorId = xml.GetValue<string>("SuccessorId");
        Backpack = xml.GetValue<bool>("Backpack");
        LDBoosted = xml.GetValue<bool>("LDBoosted");
        LTBoosted = xml.GetValue<bool>("LTBoosted");
        XpBoost = xml.GetValue<bool>("XpBoost");
        Timer = xml.GetValue<double>("Timer");
        MpEndCost = xml.GetValue<int>("MpEndCost");
        Legendary = xml.GetValue<bool>("Legendary");
        HunterLevel = xml.GetValue<int>("HunterLevel", -1);
        MaxDoses = xml.GetValue<int>("MaxDoses");
        LootBoost = xml.GetValue<double>("LootBoost");
        BoosterId = xml.GetValue<int>("BoosterId");
        UpgradeResult = xml.GetValue<int>("UpgradeResult");
        HarvestedSouls = xml.GetValue<int>("HarvestedSouls");
        UpgradeCost = xml.GetValue<int>("UpgradeCost");
        UpgradeRequirement = xml.GetValue<int>("UpgradeRequirement");
        Durability = xml.GetValue<int>("Durability");
        Cosmic = xml.GetValue<bool>("Cosmic");
        Null = xml.GetValue<bool>("Null");
        Treasure = xml.GetValue<bool>("Treasure");
        PermaPet = xml.GetValue<bool>("PermaPet");
        HF0 = xml.GetValue<bool>("HF0");
        HF1 = xml.GetValue<bool>("HF1");
        HF2 = xml.GetValue<bool>("HF2");
        HF3 = xml.GetValue<bool>("HF3");
        HF4 = xml.GetValue<bool>("HF4");
        HI0 = xml.GetValue<bool>("HI0");
        HI1 = xml.GetValue<bool>("HI1");
        HI2 = xml.GetValue<bool>("HI2");
        HI3 = xml.GetValue<bool>("HI3");
        HI4 = xml.GetValue<bool>("HI4");
        H5 = xml.GetValue<bool>("H5");
        OldSound = xml.GetValue<string>("OldSound");
        Sound = xml.GetValue<string>("Sound", "use_potion");
        Exotic = xml.GetValue<bool>("Exotic");
        Demonic = xml.GetValue<bool>("Demonic");
        Angelic = xml.GetValue<bool>("Angelic");
        TransformResult = xml.GetValue<string>("TransformResult");
        Transformed = xml.GetValue<bool>("Transformed");
        EpicKey = xml.GetValue<bool>("EpicKey");
        HalloweenLegendary = xml.GetValue<bool>("HalloweenLegendary");
        Awakened = xml.GetValue<bool>("Awakened");
        RandomEffect = xml.GetValue<bool>("RandomEffect");
        ChristmasLegendary = xml.GetValue<bool>("ChristmasLegendary");
        EasterLegendary = xml.GetValue<bool>("EasterLegendary");
        Rainbow = xml.GetValue<bool>("Rainbow");
        Held = xml.GetValue<bool>("Held");
        ShowTierTag = xml.GetValue<bool>("ShowTierTag");
        VaultItem = xml.GetValue<bool>("VaultItem");
        NotMarketable = xml.GetValue<bool>("NotMarketable");
        ReforgeStone = xml.GetValue<string>("ReforgeStone");
        NoEssences = xml.GetValue<bool>("NoEssences");
        Essences = xml.GetValue<int>("Essences");
        MaxEssences = xml.GetValue<int>("MaxEssences");
        LimitedUses = xml.GetValue<int>("LimitedUses");
        UsesLeft = xml.GetValue<int>("UsesLeft");
        CraftingMaterial = xml.GetValue<bool>("CraftingMaterial");
        LegendarySacrifices = xml.GetValue<int>("LegendarySacrifices");
        DemonicSacrifices = xml.GetValue<int>("DemonicSacrifices");
        FusionInteraction = xml.GetValue<bool>("FusionInteraction");
        Shattered = xml.GetValue<bool>("Shattered");
        RewardFor = xml.GetValue<string>("RewardFor");
        SnowballerReward = xml.GetValue<bool>("SnowballerReward");
        EffectsArePassive = xml.GetValue<bool>("EffectsArePassive");
        ReskinOf = xml.GetValue<string>("ReskinOf");
        DeathBoostAmount = xml.GetValue<int>("DeathBoostAmount");
        DropLocation = xml.GetValue<string>("DropLocation");
        BrewingItem = xml.GetValue<bool>("BrewingItem");
        BrewingEffects = xml.HasElement("BrewingEffects") ? new BrewingEffectDesc(xml.Element("BrewingEffects")) : null;
        AltTextureIndex = xml.GetValue<int>("AltTextureIndex");
        DungeonKey = xml.GetValue<bool>("DungeonKey");
        DungeonModifiers = xml.Elements("DungeonModifiers").Select(i => int.Parse(i.Value)).ToArray();
        OnlyUsableInVault = xml.GetValue<bool>("OnlyUsableInVault");
        RefundOnDeath = xml.GetValue<bool>("RefundOnDeath");
        SpecialDragInteraction = xml.GetValue<string>("SpecialDragInteraction");
        BrewingPoints = xml.GetValue<int>("BrewingPoints");
        CraftingRarity = xml.GetValue<int>("CraftingRarity");
        TabletSlots = xml.GetValue<int>("TabletSlots");
        MaxTabletSlots = xml.GetValue<int>("MaxTabletSlots");
        Projectile = xml.HasElement("Projectile") ? new ProjectileDesc(xml.Element("Projectile"), null, EssenceUpgrades) : null;
        ItemEffect = xml.Elements("ItemEffect").Select(i => int.Parse(i.Value)).ToArray();
        UseCharges = xml.GetValue<int>("UseCharges");
        MaxCharges = xml.GetValue<int>("MaxCharges");
        FlaskType = xml.GetValue<string>("FlaskType");
        CurrentCharges = xml.GetValue<int>("CurrentCharges");
        _flaskDuration = xml.GetValue<float>("FlaskDuration");
        Quality = xml.GetValue<int>("Quality");
        Enchantments = xml.Elements("Enchantments").Select(i => int.Parse(i.Value)).ToArray();
        Sealed = xml.GetValue<bool>("Sealed");
        SealedEffects = xml.Elements("SealedEffects").Select(i => new ActivateEffectDesc(i)).ToArray();
        Currency = xml.GetValue<bool>("Currency");
        OverwriteEffects = xml.Elements("OverwriteEffects").Select(i => int.Parse(i.Value)).ToArray();
        AwakenedEffects = xml.Elements("AwakenedEffects").Select(i => int.Parse(i.Value)).ToArray();
        StatBoosts = xml.Elements("ActivateOnEquip").Select(i => new StatBoostDesc(i)).ToArray();
        InventoryStatBoosts = xml.Elements("ActivateOnInventory").Select(i => new StatBoostDesc(i)).ToArray();
        Activate = xml.Elements("Activate").Select(i => new ActivateEffectDesc(i)).ToArray();
        Animation = xml.HasElement("Animation") ? new ItemAnimation(xml.Element("Animation")) : null;
        CustomToolTipDataList = xml.Elements("ExtraTooltipData").Select(i => new CustomToolTipData(i)).ToArray();
        MaskDesc = xml.HasElement("MaskDesc") ? new MaskDesc(xml.Element("MaskDesc")) : null;

        // Important to be in the end
        EssenceUpgrades.Load(xml, this);
    }

    public bool IsLegendaryPlus() {
        return Legendary || Angelic || Demonic || Exotic;
    }

    public bool IsUntiered() {
        return !IsLegendaryPlus() && HunterLevel <= 0;
    }

    public bool IsHunter() {
        return HunterLevel > 0 || HF0 || HF1;
    }

    public bool IsOrangeHunter() {
        return HF0 || HF1 || HF2 || HF3 || HF4;
    }

    public bool IsBlueHunter() {
        return HI0 || HI1 || HI2 || HI3 || HI4;
    }

    public uint GetColorFromTier() {
        switch (GetReforgeTier()) {
            case "Tiered":
                return 0x57c7ff;
            case "Untiered":
                return 0xCDA7FF;
            case "Legendary":
                return 0xf8e53c;
            case "Demonic":
                if (Demonic) {
                    return 0xfd2b59;
                }

                if (Exotic) {
                    return 0xD190DB;
                }

                if (Angelic) {
                    return 0xFFFFFF;
                }

                if (Cosmic) {
                    return 0xff38f1;
                }

                return 0xff3e67;
            default:
                return 0x57c7ff;
        }
    }

    public string GetReforgeTier() {
        if (Tier >= 0) {
            return "Tiered";
        }

        if (!Legendary && !Demonic && Tier == -1 || HunterLevel >= 0 && HunterLevel <= 3) {
            return "Untiered";
        }

        if (Legendary || HunterLevel == 4) {
            return "Legendary";
        }

        if (Exotic || Demonic || Angelic || Cosmic) {
            return "Demonic";
        }

        return null;
    }

    public void Import(IDictionary<string, object> data) {
        ParseData(this, data);
    }
}