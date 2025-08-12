using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("MaxClassLevelList")]
public class MaxClassLevelListModel {
    [XmlElement("MaxClassLevel")] public MaxClassLevelItem[] MaxClassLevels;
}