using AlloyClient.Assets.Libraries;
using AlloyClient.UiLib.Data;
using AlloyClient.UiLib.Enums;
using Common.Structs;

namespace AlloyClient.Utils;

public static class TextureHelper {
    public static TextureInfo FromUiAtlas(string lookup, int index = 0, bool padding = true) {
        var uv = Main.UiAtlas.GetAtlasData(lookup, index);
        if (!padding) uv.RemovePadding();
        return new TextureInfo(uv.ToPosition(), TextureType.UiAtlas);
    }
    
    public static TextureInfo FromGameAtlas(string lookup, int index, bool padding = true) {
        var uv = Main.Atlas.GetAtlasData(lookup, index);
        if (!padding) uv.RemovePadding();
        return new TextureInfo(uv.ToPosition(), TextureType.GameAtlas);
    }
    
    public static TextureInfo FromGameAtlas(string lookup, int index, uint removePadding) {
        var uv = Main.Atlas.GetAtlasData(lookup, index);
        uv.RemovePadding(removePadding);
        return new TextureInfo(uv.ToPosition(), TextureType.GameAtlas);
    }
    
    public static TextureInfo FromGameAtlas(ushort id) {
        if (!ObjectLibrary.TypeToTextureData.TryGetValue(id, out var data))
            return FromGameAtlas("invisible", 0);
        var uv = data.GetTexture();
        return new TextureInfo(uv.ToPosition(), TextureType.GameAtlas);
    }

    public static TextureInfo Create(AtlasData uv, TextureType type, bool padding = true) {
        if (padding) uv.RemovePadding();
        return new TextureInfo(uv.ToPosition(), type);
    }

    public static AtlasPosition ToPosition(this AtlasData data) {
        return new AtlasPosition(data.U, data.V, data.W, data.H);
    }
}