using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("ClassAvailabilityList")]
public class ClassAvailabilityModel {
    [XmlElement("ClassAvailability")] public ClassAvailabilityItem[] ClassAvailabilityItems;
}