using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AlloyClient.Assets.Libraries;

namespace AlloyClient.Editor;

public sealed class EditorCatalogEntry {
    public readonly int Type;
    public readonly string Id;
    public readonly string Name;

    public EditorCatalogEntry(int type, string id, string name) {
        Type = type;
        Id = id;
        Name = name;
    }
}

public static class EditorCatalog {
    public static readonly List<EditorCatalogEntry> Grounds = [];
    public static readonly List<EditorCatalogEntry> Objects = [];
    public static readonly List<EditorCatalogEntry> Regions = [];

    private static readonly Dictionary<int, string> GroundIds = [];
    private static readonly Dictionary<int, string> ObjectIds = [];
    private static readonly Dictionary<int, string> RegionIds = [];
    private static readonly Dictionary<string, int> RegionTypes = new(StringComparer.Ordinal);
    private static bool _loaded;

    public static void Load() {
        if (_loaded) return;
        _loaded = true;

        foreach (var pair in GroundLibrary.TypeToGroundProps.OrderBy(pair => pair.Value.ObjectId)) {
            var entry = new EditorCatalogEntry(pair.Key, pair.Value.ObjectId, pair.Value.ObjectId);
            Grounds.Add(entry);
            GroundIds[pair.Key] = pair.Value.ObjectId;
        }

        foreach (var pair in ObjectLibrary.TypeToObjectProps.OrderBy(pair => pair.Value.DisplayName)) {
            if (!ObjectLibrary.TypeToTextureData.ContainsKey(pair.Key)) continue;
            var entry = new EditorCatalogEntry(pair.Key, pair.Value.ObjectId, pair.Value.DisplayName);
            Objects.Add(entry);
            ObjectIds[pair.Key] = pair.Value.ObjectId;
        }

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Xmls", "Regions.xml");
        if (!File.Exists(path)) return;
        foreach (var element in XDocument.Load(path).Root?.Elements("Region") ?? []) {
            var typeText = element.Attribute("type")?.Value ?? "0";
            var hexadecimal = typeText.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            var type = Convert.ToInt32(hexadecimal ? typeText[2..] : typeText, hexadecimal ? 16 : 10);
            var id = element.Attribute("id")?.Value ?? typeText;
            Regions.Add(new EditorCatalogEntry(type, id, id));
            RegionIds[type] = id;
            RegionTypes[id] = type;
        }
    }

    public static List<EditorCatalogEntry> GetEntries(EditorDrawType drawType) {
        Load();
        return drawType switch {
            EditorDrawType.Ground => Grounds,
            EditorDrawType.Objects => Objects,
            EditorDrawType.Regions => Regions,
            _ => Grounds
        };
    }

    public static string GetId(EditorDrawType drawType, int type) {
        Load();
        var ids = drawType switch {
            EditorDrawType.Ground => GroundIds,
            EditorDrawType.Objects => ObjectIds,
            EditorDrawType.Regions => RegionIds,
            _ => GroundIds
        };
        return ids.GetValueOrDefault(type, string.Empty);
    }

    public static int GetType(EditorDrawType drawType, string id) {
        Load();
        if (string.IsNullOrEmpty(id)) return drawType == EditorDrawType.Ground ? -1 : 0;
        return drawType switch {
            EditorDrawType.Ground when GroundLibrary.IdToTileType.TryGetValue(id, out var type) => type,
            EditorDrawType.Objects when ObjectLibrary.IdToObjectType.TryGetValue(id, out var type) => type,
            EditorDrawType.Regions when RegionTypes.TryGetValue(id, out var type) => type,
            _ => drawType == EditorDrawType.Ground ? -1 : 0
        };
    }
}
