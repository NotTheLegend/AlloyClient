using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class CustomToolTipData : ItemData {

    public string Name;
    public string Description;
    public string NameColorString;
    public uint NameColorUint;
    public string DescriptionColorString;
    public uint DescriptionColorUint;
    public bool HideWhenScaled;

    public CustomToolTipData(XElement xml) {
        Name = xml.GetAttribute<string>("name");
        Description = xml.GetAttribute<string>("description");
        NameColorString = ColorToString(xml.GetAttribute<string>("nameColor"));
        DescriptionColorString = ColorToString(xml.GetAttribute<string>("descriptionColor"));
        NameColorUint = ColorToUint(xml.GetAttribute<string>("nameColorUint"));
        DescriptionColorUint = ColorToUint(xml.GetAttribute<string>("descriptionColorUint"));
        HideWhenScaled = xml.HasAttribute("hideWhenScaled");
    }

    private static string ColorToString(string color) {
        if (string.IsNullOrEmpty(color))
            return "";
        
        if (color.Contains("0x")) {
            color = "#" + color.Replace("0x", "");
        }

        return color;
    }

    private static uint ColorToUint(string color) {
        return string.IsNullOrEmpty(color) ? 0 : uint.Parse(color.Replace("#", "0x"));
    }
}