using System.Xml.Serialization;

namespace RealmClient.Data.XmlModels;

[XmlRoot("Stats")]
public class StatsModel {
    [XmlElement("ClassStats")] public ClassStatsModel[] ClassStats;

    [XmlElement("BestCharFame")] public int BestCharFame;

    [XmlElement("TotalFame")] public int TotalFame;

    [XmlElement("Fame")] public int Fame;
}