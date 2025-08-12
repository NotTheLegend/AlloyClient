using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("News")]
public class NewsListModel {
    [XmlElement("Item")] public NewsListItem[] Items;
}