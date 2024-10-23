using System.Xml.Linq;
using Common;

namespace MonoClient.Assets.XmlStructs;

public class AnimatedGround {
    public float X;
    public float Y;
    
    public AnimatedGround(XElement e) {
        X = e.GetAttribute<float>("dx");
        Y = e.GetAttribute<float>("dy");
    }
}