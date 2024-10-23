using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("ClassAvailabilityList")]
public class ClassAvailabilityModel {
    [XmlElement("ClassAvailability")] public ClassAvailabilityItem[] ClassAvailabilityItems;
}