using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("Account")]
public class AccountModel {
    [XmlElement("AccountId")] public long AccountId;

    [XmlElement("Name")] public string Name;
    
    [XmlElement("Credits")] public int Credits;

    [XmlElement("Souls")] public int Souls;

    [XmlElement("MenuMusic")] public string MenuMusic;

    [XmlElement("DeadMusic")] public string DeadMusic;

    [XmlElement("NextCharSlotPrice")] public int NextCharSlotPrice;

    [XmlElement("NextCharSlotCurrency")] public string NextCharSlotCurrency;

    [XmlElement("Stats")] public StatsModel Stats;

    [XmlElement("Guild")] public GuildModel Guild;
}