using System.Xml.Linq;
using Common;

namespace MonoClient.Assets.XmlStructs;

public class TextureIndex {
    public readonly string File;
    public readonly ushort Index;

    public TextureIndex(XElement e) {
        File = e.GetValue<string>("File");
        Index = e.GetValue<ushort>("Index");
    }
}