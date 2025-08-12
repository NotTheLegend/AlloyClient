using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("ServerItem")]
public class ServerListItem {
    [XmlElement("Name")] public string Name;

    [XmlElement("Dns")] public string Dns;

    [XmlElement("Port")] public int Port;

    [XmlElement("Lat")] public double Lat;

    [XmlElement("Long")] public double Long;

    [XmlElement("Players")] public int Players;

    [XmlElement("MaxPlayers")] public int MaxPlayers;

    [XmlElement("AdminOnly")] public bool AdminOnly;

    [XmlElement("CurrentBackup")] public int CurrentBackup;
}