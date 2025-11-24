using System.Collections.Generic;
using RealmClient.Assets.XmlStructs;

namespace RealmClient.Assets.Libraries;

public static class GroundLibrary {
    public static readonly Dictionary<ushort, GroundProperties> TypeToGroundProps = new();
    public static readonly Dictionary<ushort, TextureData> TypeToTextureData = new();
    public static readonly Dictionary<string, ushort> IdToTileType = new();
}