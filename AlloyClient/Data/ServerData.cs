using System.Linq;
using System.Xml.Linq;
using Common;

namespace AlloyClient.Data;

public sealed class ServerListData(XElement xml) : IGlobalData {

    public readonly ServerItem[] ServerList = xml.Elements("Server").Select(s => new ServerItem(s)).ToArray();
    
}

public sealed class ServerItem(XElement xml) {
    
    public readonly string Name = xml.GetValue("Name", "");

    public readonly string Dns = xml.GetValue("DNS", "");

    public readonly int Port = xml.GetValue("Port", 2050);

    public readonly int Players = xml.GetValue("Players", 0);

    public readonly int MaxPlayers = xml.GetValue("MaxPlayers", 0);

    public readonly bool AdminOnly = xml.GetValue("AdminOnly", false);
}