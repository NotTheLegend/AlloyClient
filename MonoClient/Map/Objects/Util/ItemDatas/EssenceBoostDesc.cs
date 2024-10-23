using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class EssenceBoostDesc : ItemData {

    public string Type;
    public int Stat;
    public float Amount;
    public string CustomName;
    public string CustomAmount;

    public EssenceBoostDesc(XElement xml) {
        Type = xml.GetAttribute<string>("type");
        Stat = xml.GetAttribute<int>("stat");
        Amount = xml.GetAttribute<float>("amount");
        CustomName = xml.GetAttribute<string>("customName");
        CustomAmount = xml.GetAttribute<string>("customAmount");
    }
}