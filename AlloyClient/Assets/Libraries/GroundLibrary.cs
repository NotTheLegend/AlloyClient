using System.Collections.Generic;
using AlloyClient.Assets.XmlStructs;

namespace AlloyClient.Assets.Libraries;

public static class GroundLibrary {
    public static readonly Dictionary<ushort, GroundProperties> TypeToGroundProps = new();
    public static readonly Dictionary<ushort, TextureData> TypeToTextureData = new();
    public static readonly Dictionary<string, ushort> IdToTileType = new();
}