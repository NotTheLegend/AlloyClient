using System.Xml.Serialization;

namespace MonoClient.Data.XmlModels;

[XmlRoot("ItemCosts")]
public class ItemCostListModel {
    [XmlElement("ItemCost")] public int ItemCost;

    [XmlElement("Type")] public ushort Type;
}