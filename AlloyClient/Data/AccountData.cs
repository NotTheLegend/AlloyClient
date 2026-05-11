using System.Collections.Generic;
using System.Xml.Linq;
using Alloy.Common;

namespace AlloyClient.Data;

public sealed class AccountData : IGlobalData {
    public readonly int AccountId;

    public readonly int Rank;

    public readonly string Name;

    public readonly bool Admin;

    public readonly string GuildName;

    public readonly int GuildRank;

    public readonly AccountStats Stats;

    public AccountData(XElement xml) {
        AccountId = xml.GetValue("AccountId", -1);
        Rank = xml.GetValue("Rank", 0);
        Name = xml.GetValue("Name", "");

        if (xml.HasElement("Guild")) {
            var guild = xml.Element("Guild");
            GuildName = guild.GetValue("Name", "");
            GuildRank = guild.GetValue("Rank", 0);
        }

        Admin = xml.HasElement("Admin");
        
        Stats = new AccountStats(xml.Element("Stats"));
    }
}

public sealed class AccountStats {

    public readonly int BestCharacterFame;

    public readonly int TotalFame;

    public readonly int Fame;

    public readonly int TotalCredits;

    public readonly int Credits;

    public readonly int TotalGuildFame;

    public readonly int GuildFame;

    public readonly ClassStats[] ClassStats;


    public AccountStats(XElement xml) {
        BestCharacterFame = xml.GetValue("BestCharFame", 0);
        TotalFame = xml.GetValue("TotalFame", 0);
        Fame = xml.GetValue("Fame", 0);
        TotalCredits = xml.GetValue("TotalCredits", 0);
        Credits = xml.GetValue("Credits", 0);
        TotalGuildFame = xml.GetValue("TotalGuildFame", 0);
        GuildFame = xml.GetValue("GuildFame", 0);

        var classStats = new List<ClassStats>();
        foreach (var stats in xml.Elements("ClassStats")) {
            classStats.Add(new ClassStats(stats));
        }

        ClassStats = classStats.ToArray();
    }
}

public class ClassStats(XElement xml) {

    public readonly int ObjectType = xml.GetValue("objectType", 0);

    public readonly int BestFame = xml.GetValue("BestFame", 0);

    public readonly int BestLevel = xml.GetValue("BestLevel", 0);
}