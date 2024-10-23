using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Common;
using MonoClient.Networking.Enums;

namespace MonoClient.Objects.Util.ItemDatas;

public class EssenceUpgradeManager {

    public List<EssenceBoostDesc> Upgrades = [];
    public float DamageBoost;
    public Dictionary<int, float> StatBoosts = new();
    public float ArcGap;
    public float RateOfFire;
    public float Amplitude;
    public float MpCost;
    public float AbilityDamage;
    public float Cooldown;

    public void Load(XElement xml, ItemDesc itemDesc) {
        Upgrades.Clear();

        if (xml.HasElement("EssenceBoost")) {
            foreach (var x in xml.Elements("EssenceBoost")) {
                Upgrades.Add(new EssenceBoostDesc(x));
            }
        }

        var essences = Math.Min(itemDesc.MaxEssences, itemDesc.Essences);
        var avgDamage = 0;
        if (itemDesc.Projectile != null) {
            avgDamage = (itemDesc.Projectile.MinDamage + itemDesc.Projectile.MaxDamage) / 2;
        }

        foreach (var boost in Upgrades) {
            switch (boost.Type) {
                case "Damage":
                    DamageBoost += MathF.Round(avgDamage * boost.Amount * essences);
                    break;
                case "IncrementStat":
                    StatBoosts.TryAdd(boost.Stat, 0);

                    StatBoosts[boost.Stat] += boost.Amount * essences;
                    break;
                case "ArcGap":
                    ArcGap += boost.Amount * essences;
                    break;
                case "RateOfFire":
                    RateOfFire += boost.Amount * essences;
                    break;
                case "Amplitude":
                    Amplitude += boost.Amount * essences;
                    break;
                case "MpCost":
                    MpCost += boost.Amount * essences;
                    break;
                case "Custom":
                    break;
                case "AbilityDamage":
                    AbilityDamage += boost.Amount * essences;
                    break;
                case "Cooldown":
                    Cooldown += boost.Amount * essences;
                    break;
            }
        }
    }

    public string GetUpgradesString() {
        var str= "";
        foreach (var boost in Upgrades) {
            switch (boost.Type) {
                case "Damage":
                    str += Round(boost.Amount * 100) + "% Damage";
                    break;
                case "IncrementStat":
                    str += StatsUtil.Convert((StatsType)boost.Stat, boost.Amount)
                           + StatsUtil.GetSign(boost.Stat) + " " + StatsUtil.FromId(boost.Stat);
                    break;
                case "ArcGap":
                    str += boost.Amount + " " + "Arc Gap";
                    break;
                case "RateOfFire":
                    str += Round(boost.Amount * 100) + "% Rate of Fire";
                    break;
                case "Amplitude":
                    str += boost.Amount + " Shot Amplitude";
                    break;
                case "MpCost":
                    str += boost.Amount + " MP Cost";
                    break;
                case "Custom":
                    float amount = boost.Amount;
                    str += (amount == 0 ? boost.CustomAmount : amount) + " " + boost.CustomName;
                    break;
                case "AbilityDamage":
                    str += boost.Amount + " Ability Damage";
                    break;
                case "Cooldown":
                    str += boost.Amount + " secs Cooldown";
                    break;
            }

            if (Upgrades.IndexOf(boost) < Upgrades.Count - 1)
                str += ", ";
        }

        return str;
    }
    
    private static float Round(float value) {
        return (int)(value * 100) / 100.0f;
    }
}