using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("Servers")]
public class ServerListModel {
    [XmlElement("Server")] public ServerListItem[] ServerItems;
}