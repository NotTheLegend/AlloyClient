using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("Guild")]
public class GuildModel {
    [XmlElement("Id")] public string Id;

    [XmlElement("Name")] public string Name;

    [XmlElement("Rank")] public int Rank;
}