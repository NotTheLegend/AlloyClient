using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("ClassAvailability")]
public class ClassAvailabilityItem {
    [XmlAttribute("id")] public string Id;
}