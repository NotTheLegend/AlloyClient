using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class StatBoostDesc : ItemData {

    public int Stat;
    public int Amount;
    public bool AwakenedOnly;

    public StatBoostDesc(XElement xml) {
        Stat = xml.GetValue<int>("Stat");
        Amount = xml.GetValue<int>("Amount");
        AwakenedOnly = xml.GetValue<bool>("AwakenedOnly");
    }
}