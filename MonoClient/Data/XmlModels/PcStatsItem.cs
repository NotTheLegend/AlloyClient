using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("PcStats")]
public class PcStatsItem {
    [XmlElement("Shots")] public int Shots;

    [XmlElement("ShotsThatDamage")] public int ShotsThatDamage;

    [XmlElement("SpecialAbilityUses")] public int SpecialAbilityUses;

    [XmlElement("TilesUncovered")] public int TilesUncovered;

    [XmlElement("Teleports")] public int Teleports;

    [XmlElement("LevelUpAssists")] public int LevelUpAssists;

    [XmlElement("PotionsDrunk")] public int PotionsDrunk;

    [XmlElement("MonsterKills")] public int MonsterKills;

    [XmlElement("MonsterAssists")] public int MonsterAssists;

    [XmlElement("GodKills")] public int GodKills;

    [XmlElement("GodAssists")] public int GodAssists;

    [XmlElement("CubeKills")] public int CubeKills;

    [XmlElement("OryxKills")] public int OryxKills;

    [XmlElement("QuestsCompleted")] public int QuestsCompleted;
}