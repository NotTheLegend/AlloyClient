namespace AlloyClient.Game.Objects.Util;

public static class StatsUtil {

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
            _ => "Invalid Stat!"
        };
    }
}