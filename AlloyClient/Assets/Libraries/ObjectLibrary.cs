using System;
using System.Collections.Generic;
using RealmClient.Assets.XmlStructs;

namespace RealmClient.Assets.Libraries;

public static class ObjectLibrary {
    public static readonly Dictionary<ushort, ObjectProperties> TypeToObjectProps = new();
    public static readonly Dictionary<ushort, PlayerProperties> TypeToClassProps = new();
    public static readonly List<Tuple<ushort, ushort>> TypeToSkins = new();
    public static readonly Dictionary<ushort, TextureData> TypeToTextureData = new();

    public static readonly Dictionary<string, ushort> IdToObjectType = new();

    public static readonly Dictionary<ushort, ItemDesc> TypeToItem = new();

    public static ItemDesc GetItem(ushort type) => type == 0 ? null : TypeToItem[type];
}