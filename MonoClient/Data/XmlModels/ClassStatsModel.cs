using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("ClassStats")]
public class ClassStatsModel {
    [XmlAttribute("objectType")] public string ObjectType;

    [XmlElement("BestLevel")] public int BestLevel;

    [XmlElement("BestFame")] public int BestFame;
}