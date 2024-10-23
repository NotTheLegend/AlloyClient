using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class ParticleTrailDesc : ItemData {

    public uint Color;
    public int LifetimeMS;
    public int Size;
    
    public ParticleTrailDesc(XElement xml) {
        Color = uint.Parse(xml.Value);
        LifetimeMS = xml.GetAttribute<int>("lifetimeMS");
        Size = xml.GetAttribute<int>("size");
    }
}