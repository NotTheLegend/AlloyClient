using System.Collections.Generic;
using AlloyClient.Assets.XmlStructs;

namespace AlloyClient.Assets.Libraries;

public static class GroundLibrary {
    public readonly static Dictionary<ushort, GroundProperties> TypeToGroundProps = new();
    public readonly static Dictionary<ushort, TextureData> TypeToTextureData = new();
    public readonly static Dictionary<string, ushort> IdToTileType = new();
}