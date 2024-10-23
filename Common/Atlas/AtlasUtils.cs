using Microsoft.Xna.Framework.Content;

namespace Common.Atlas;

public static class AtlasUtils {
    public static AtlasData ReadAtlasData(this ContentReader reader) {
        return new AtlasData {
            U = reader.ReadSingle(),
            V = reader.ReadSingle(),
            W = reader.ReadSingle(),
            H = reader.ReadSingle()
        };
    }
}