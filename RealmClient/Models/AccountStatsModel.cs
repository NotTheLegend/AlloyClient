using System.Collections.Generic;
using System.Xml.Linq;
using Common;

namespace RealmClient.Models;

public class AccountStatsModel {

    public int BestCharacterFame { get; private set; }

    public int TotalFame { get; private set; }

    public int Fame { get; private set; }

    public int TotalCredits { get; private set; }

    public int Credits { get; private set; }

    public int TotalGuildFame { get; private set; }

    public int GuildFame { get; private set; }

    public ClassStatsModel[] ClassStats { get; private set; } = [];


    public void ParseXml(XElement xml) {
        BestCharacterFame = xml.GetValue("BestCharFame", 0);
        TotalFame = xml.GetValue("TotalFame", 0);
        Fame = xml.GetValue("Fame", 0);
        TotalCredits = xml.GetValue("TotalCredits", 0);
        Credits = xml.GetValue("Credits", 0);
        TotalGuildFame = xml.GetValue("TotalGuildFame", 0);
        GuildFame = xml.GetValue("GuildFame", 0);

        var classStats = new List<ClassStatsModel>();
        foreach (var stats in xml.Elements("ClassStats")) {
            classStats.Add(new ClassStatsModel(stats));
        }

        ClassStats = classStats.ToArray();
    }

    public void Reset() {
        BestCharacterFame = 0;
        TotalFame = 0;
        Fame = 0;
        TotalCredits = 0;
        Credits = 0;
        TotalGuildFame = 0;
        GuildFame = 0;
        ClassStats = [];
    }
}

public class ClassStatsModel(XElement xml) {

    public int ObjectType { get; } = xml.GetValue("objectType", 0);

    public int BestFame { get; } = xml.GetValue("BestFame", 0);

    public int BestLevel { get; } = xml.GetValue("BestLevel", 0);
}