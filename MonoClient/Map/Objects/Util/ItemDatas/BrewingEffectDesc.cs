using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class BrewingEffectDesc : ItemData {

    public int NutrientValue;
    public int Instability;
    public string Rarity;
    public string Description;

    public BrewingEffectDesc(XElement xml) {
        NutrientValue = xml.GetValue<int>("NutrientValue");
        Instability = xml.GetValue<int>("Instability");
        Rarity = xml.GetValue<string>("Rarity");
        Description = xml.GetValue<string>("Description");
    }
}