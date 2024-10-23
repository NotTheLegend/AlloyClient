using System.Xml.Linq;
using Common;

namespace MonoClient.Assets.XmlStructs;

public class AnimatedTexture {
    public readonly string File;
    public readonly ushort Index;
    
    public AnimatedTexture(XElement e) {
        File = e.GetValue<string>("File");
        Index = e.GetValue<ushort>("Index");
    }
}