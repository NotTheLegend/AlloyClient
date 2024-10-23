using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class ItemAnimationFrame : ItemData {

    public float Time;
    public string File;
    public int Index;
    
    public ItemAnimationFrame(XElement xml) {
        Time = xml.GetAttribute<float>("time");
        File = xml.GetValue<string>("File");
        Index = xml.GetValue<int>("Index");
    }
}