using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("Item")]
public class NewsListItem {
    [XmlElement("Icon")] public string Icon;

    [XmlElement("Title")] public string Title;

    [XmlElement("TagLine")] public string TagLine;

    [XmlElement("Link")] public string Link;

    [XmlElement("Data")] public string Data;
}