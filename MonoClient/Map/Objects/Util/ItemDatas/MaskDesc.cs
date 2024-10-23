using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class MaskDesc : ItemData {

    public ushort SkinType;
    public float Duration;
    public Dictionary<int, int> MaskBoosts;

    public MaskDesc(XElement xml) {
        SkinType = xml.GetValue<ushort>("SkinType");
        Duration = xml.GetValue<float>("Duration");
        if (xml.HasElement("MaskBoost")) {
            MaskBoosts = new Dictionary<int, int>();
            foreach (var x in xml.Elements("MaskBoost")) {
                var stat = x.GetAttribute<int>("stat");
                var amount = x.GetAttribute<int>("amount");
                MaskBoosts[stat] = amount;
            }
        }
    }
}