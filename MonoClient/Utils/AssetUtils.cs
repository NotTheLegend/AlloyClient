using MonoClient.Assets.Libraries;
using MonoClient.UiLib.Assets;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Utils;

public static class AssetUtils {
    public static TextureInfo GetTextureInfo(ushort id) {
        if (!ObjectLibrary.TypeToTextureData.TryGetValue(id, out var uv))
            return TextureInfo.FromGameAtlas("invisible", 0);
        
        return new TextureInfo(uv.GetTexture(), TextureType.GameAtlas);
    }
}