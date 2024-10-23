using System.Linq;
using System.Xml.Linq;
using Common;

namespace MonoClient.Objects.Util.ItemDatas;

public class ItemAnimation : ItemData {

    public ItemAnimationFrame[] Frames;

    public ItemAnimation(XElement xml) {
        Frames = xml.HasElement("Frame") ? xml.Elements("Frame").Select(x => new ItemAnimationFrame(x)).ToArray() : null;
    }
}