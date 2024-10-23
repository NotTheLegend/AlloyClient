using Common.Atlas;
using MonoClient.UiLib.Enums;

namespace MonoClient.UiLib.Core;

public struct TextureInfo {
    public readonly AtlasData AtlasData;
    public readonly TextureType TextureType;

    public TextureInfo(AtlasData data, TextureType type) {
        AtlasData = data;
        TextureType = type;
    }

    public static TextureInfo FromUiAtlas(string lookup, int index = 0) {
        var uv = UiRender.UiAtlas.GetAtlasData(lookup, index);
        return new TextureInfo(uv, TextureType.UiAtlas);
    }
    
    public static TextureInfo FromGameAtlas(string lookup, int index) {
        var uv = UiRender.GameAtlas.GetAtlasData(lookup, index);
        return new TextureInfo(uv, TextureType.GameAtlas);
    }
}