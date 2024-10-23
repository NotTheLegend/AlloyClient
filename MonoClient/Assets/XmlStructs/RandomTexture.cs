using System.Linq;
using System.Xml.Linq;

namespace MonoClient.Assets.XmlStructs;

public class RandomTexture {
    public readonly TextureIndex[] Textures;
    
    public RandomTexture(XElement e) {
        var textures = e.Elements("Texture").ToArray();
        Textures = new TextureIndex[textures.Length];
        for (var i = 0; i < textures.Length; i++) {
            Textures[i] = new TextureIndex(textures[i]);
        }
    }
}