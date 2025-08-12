using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("Chars")]
public class CharacterListModel {
    [XmlAttribute("nextCharId")] public int NextCharId;

    [XmlAttribute("maxNumChars")] public int MaxNumChars;

    [XmlElement("Char")] public CharacterModel[] Characters;

    [XmlElement("Account")] public AccountModel Account;

    [XmlElement("ClassAvailability")] public ClassAvailabilityModel ClassAvailability;

    [XmlElement("News")] public NewsListModel NewsList;

    [XmlElement("Servers")] public ServerListModel Servers;

    [XmlElement("Lat")] public double Lat;

    [XmlElement("Long")] public double Long;

    [XmlElement("OwnedSkins")] public string OwnedSkins;

    [XmlElement("ItemCosts")] public ItemCostListModel ItemCosts;

    [XmlElement("MaxClassLevelList")] public MaxClassLevelListModel MaxClassLevelList;
}