using MonoClient.UiLib.Enums;

namespace MonoClient.UiLib.Data;

public record struct TextureInfo(AtlasPosition AtlasPosition, TextureType TextureType);

public record struct AtlasPosition(float U, float V, float W, float H);