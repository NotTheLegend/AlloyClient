using System;
using System.Collections.Generic;
using AlloyClient.Assets.XmlStructs;

namespace AlloyClient.Assets.Libraries;

public static class ObjectLibrary {
    public readonly static Dictionary<ushort, ObjectProperties> TypeToObjectProps = new();
    public readonly static Dictionary<ushort, PlayerProperties> TypeToClassProps = new();
    public readonly static List<Tuple<ushort, ushort>> TypeToSkins = new();
    public readonly static Dictionary<ushort, TextureData> TypeToTextureData = new();

    public readonly static Dictionary<string, ushort> IdToObjectType = new();

    public readonly static Dictionary<ushort, ItemDesc> TypeToItem = new();

    public static ItemDesc GetItem(ushort type) => type == 0 ? null : TypeToItem[type];
}