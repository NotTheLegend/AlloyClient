using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("News")]
public class NewsListModel {
    [XmlElement("Item")] public NewsListItem[] Items;
}