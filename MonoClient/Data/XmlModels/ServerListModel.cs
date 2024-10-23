using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("Servers")]
public class ServerListModel {
    [XmlElement("Server")] public ServerListItem[] ServerItems;
}