using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.Assets.XmlStructs;
using MonoClient.Objects.Util;
using MonoClient.Objects.Util.ItemDatas;

namespace MonoClient.Assets.Libraries;

public static class ObjectLibrary {
    public static readonly Dictionary<ushort, ObjectProperties> TypeToObjectProps = new();
    public static readonly Dictionary<ushort, PlayerProperties> TypeToClassProps = new();
    public static readonly List<Tuple<ushort, ushort>> TypeToSkins = new();
    public static readonly Dictionary<ushort, TextureData> TypeToTextureData = new();

    public static readonly Dictionary<string, ushort> IdToObjectType = new();
    
    public static readonly Dictionary<ushort, XElement> ItemXmls = new();

    public static ItemDesc CreateItem(ushort type) {
        if (!ItemXmls.TryGetValue(type, out var xml)) {
            return null;
        }

        return new ItemDesc(xml);
    }
}