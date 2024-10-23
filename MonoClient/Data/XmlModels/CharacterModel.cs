using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("Char")]
public class CharacterModel {
    [XmlAttribute("id")] public int Id;

    [XmlElement("ObjectType")] public ushort ObjectType;

    [XmlElement("Level")] public int Level;

    [XmlElement("Exp")] public int Exp;

    [XmlElement("CurrentFame")] public int CurrentFame;

    [XmlElement("ItemDatas")] public string ItemDatas;

    [XmlElement("MaxHitPoints")] public int MaxHitPoints;

    [XmlElement("HitPoints")] public int HitPoints;

    [XmlElement("MaxMagicPoints")] public int MaxMagicPoints;

    [XmlElement("MagicPoints")] public int MagicPoints;

    [XmlElement("Attack")] public int Attack;

    [XmlElement("Defense")] public int Defense;

    [XmlElement("Speed")] public int Speed;

    [XmlElement("Dexterity")] public int Dexterity;

    [XmlElement("HpRegen")] public int HpRegen;

    [XmlElement("MpRegen")] public int MpRegen;

    [XmlElement("Tex1")] public int Tex1;

    [XmlElement("Tex2")] public int Tex2;

    [XmlElement("Texture")] public ushort Skin;

    [XmlElement("HealthStackCount")] public int HealthStackCount;

    [XmlElement("MagicStackCount")] public int MagicStackCount;

    [XmlElement("Dead")] public bool Dead;

    [XmlElement("HasBackpack")] public bool HasBackpack;

    [XmlElement("Birthsign")] public string BirthSign;
    
    [XmlElement("SkillLevel")] public int SkillLevel;

    [XmlElement("ReviveUsed")] public bool ReviveUsed;

    [XmlElement("PcStats")] public PcStatsItem PcStats;
    
}