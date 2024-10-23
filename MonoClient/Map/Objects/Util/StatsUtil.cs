using MonoClient.Networking.Enums;
using MonoClient.Ui.Components.Tooltips;

namespace MonoClient.Objects.Util;

public static class StatsUtil {

    public static float Convert(StatsType statId, float amount) {
        switch (statId) {
            case StatsType.DamageReduction:
                return EquipmentToolTip.Round(amount / 10.0f, 1);
            case StatsType.CriticalChance:
                return EquipmentToolTip.Round(amount / 10.0f, 1);
            case StatsType.CriticalMultiplier:
                return EquipmentToolTip.Round(amount / 10.0f, 1);
            case StatsType.DodgeChance:
                return EquipmentToolTip.Round(amount / 10.0f, 1);
            case StatsType.ShieldRechargeTime:
                return EquipmentToolTip.Round(amount / 1000.0f, 2);
            case StatsType.MovementSpeed:
                return EquipmentToolTip.Round(amount, 1);
        }
        return amount;
    }

    public static string FromId(int statId) {
        return statId switch {
            0 => "Maximum HP",
            3 => "Maximum MP",
            20 => "Attack",
            21 => "Defense",
            22 => "Speed",
            28 => "Dexterity",
            26 => "Vitality",
            27 => "Wisdom",
            116 => "Maximum Shield",
            125 => "Loot Boost",
            132 => "Damage Reduction",
            135 => "Attack Speed",
            138 => "Critical Hit Chance",
            139 => "Critical Hit Multiplier",
            140 => "Dodge Chance",
            143 => "Mana Regen",
            144 => "Shield Recharge Time",
            165 => "Movement Speed",
            _ => "Invalid Stat!"
        };
    }

    public static string GetSign(int statId) {
        return statId switch {
            125 => "%",
            132 => "%",
            135 => "%",
            138 => "%",
            139 => "%",
            140 => "%",
            143 => "%",
            144 => "s",
            165 => "%",
            _ => ""
        };
    }
}