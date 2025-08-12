using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("MaxClassLevel")]
public class MaxClassLevelItem {
    [XmlAttribute("classType")] public string ClassType;

    [XmlAttribute("maxLevel")] public int MaxLevel;
}