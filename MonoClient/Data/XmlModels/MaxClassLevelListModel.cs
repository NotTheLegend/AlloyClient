using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("MaxClassLevelList")]
public class MaxClassLevelListModel {
    [XmlElement("MaxClassLevel")] public MaxClassLevelItem[] MaxClassLevels;
}