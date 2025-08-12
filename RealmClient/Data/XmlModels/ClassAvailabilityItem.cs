using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("ClassAvailability")]
public class ClassAvailabilityItem {
    [XmlAttribute("id")] public string Id;
}