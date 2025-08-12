using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("Account")]
public class AccountModel {
    [XmlElement("AccountId")] public long AccountId;

    [XmlElement("Name")] public string Name;
    
    [XmlElement("Credits")] public int Credits;
    
    [XmlElement("NameChosen", IsNullable = true)] private string _nameChosen;

    public bool NameChosen => _nameChosen != null;
    
    [XmlElement("Admin", IsNullable = true)] private string _admin;
    
    public bool Admin => _admin != null;

    [XmlElement("NextCharSlotPrice")] public int NextCharSlotPrice;

    [XmlElement("NextCharSlotCurrency")] public string NextCharSlotCurrency;

    [XmlElement("Stats")] public StatsModel Stats;

    [XmlElement("Guild")] public GuildModel Guild;
}