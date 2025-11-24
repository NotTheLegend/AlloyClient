using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Common;

namespace AlloyClient.Data;

public sealed class NewsData(IEnumerable<XElement> xmls) : IGlobalData {
    public readonly NewsItem[] NewsList = xmls.Select(n => new NewsItem(n)).ToArray();
}

public sealed class NewsItem(XElement xml) {
    public readonly string Icon = xml.GetValue("Icon", "");

    public readonly string Title = xml.GetValue("Title", "");

    public readonly string TagLine = xml.GetValue("TagLine", "");

    public readonly string Link = xml.GetValue("Link", "");

    public readonly int Date = xml.GetValue("Date", 0);
}