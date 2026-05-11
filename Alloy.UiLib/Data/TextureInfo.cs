using Alloy.UiLib.Enums;

namespace Alloy.UiLib.Data;

public record struct TextureInfo(AtlasPosition AtlasPosition, TextureType TextureType);

public record struct AtlasPosition(float U, float V, float W, float H);