using Common.Atlas;
using MonoClient.UiLib.Enums;

namespace MonoClient.UiLib.Data;

public record struct TextureInfo(AtlasData AtlasData, TextureType TextureType) {
    //Todo: move functions to main project?
    public static TextureInfo FromUiAtlas(string lookup, int index = 0) {
        var uv = UiRender.UiAtlas.GetAtlasData(lookup, index);
        return new TextureInfo(uv, TextureType.UiAtlas);
    }
    
    public static TextureInfo FromGameAtlas(string lookup, int index) {
        var uv = UiRender.GameAtlas.GetAtlasData(lookup, index);
        return new TextureInfo(uv, TextureType.GameAtlas);
    }
}