using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AlloyClient.Editor;

public static class EditorMapSerializer {
    private const byte WmapVersion = 1;

    public static EditorMapData Load(string path) {
        var extension = Path.GetExtension(path);
        return extension.Equals(".wmap", StringComparison.OrdinalIgnoreCase)
            ? LoadWmap(path)
            : LoadJson(path);
    }

    public static void Save(EditorMapData map, string path) {
        if (Path.GetExtension(path).Equals(".wmap", StringComparison.OrdinalIgnoreCase)) SaveWmap(map, path);
        else SaveJson(map, path);

        map.SavedChanges = true;
    }

    public static EditorMapData LoadJson(string path) {
        EditorCatalog.Load();
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var width = root.GetProperty("width").GetInt32();
        var height = root.GetProperty("height").GetInt32();
        var map = new EditorMapData(Path.GetFileNameWithoutExtension(path), width, height);
        var dictionary = root.GetProperty("dict");
        var compressed = Convert.FromBase64String(root.GetProperty("data").GetString() ?? string.Empty);
        using var source = new MemoryStream(compressed);
        using var zlib = new ZLibStream(source, CompressionMode.Decompress);
        using var decoded = new MemoryStream();
        zlib.CopyTo(decoded);
        var indices = decoded.ToArray();
        var requiredBytes = checked(map.Tiles.Length * sizeof(ushort));
        if (indices.Length < requiredBytes)
            throw new InvalidDataException("JM tile data is shorter than the declared map dimensions.");

        var littleEndianScore = CountValidIndices(indices, map.Tiles.Length, dictionary.GetArrayLength(), false);
        var bigEndianScore = CountValidIndices(indices, map.Tiles.Length, dictionary.GetArrayLength(), true);
        var bigEndian = bigEndianScore > littleEndianScore;

        for (var i = 0; i < map.Tiles.Length; i++) {
            var bytes = indices.AsSpan(i * sizeof(ushort), sizeof(ushort));
            var index = bigEndian
                ? BinaryPrimitives.ReadUInt16BigEndian(bytes)
                : BinaryPrimitives.ReadUInt16LittleEndian(bytes);

            if (index >= dictionary.GetArrayLength()) continue;

            ReadJsonTile(dictionary[index], map.Tiles[i]);
        }

        map.SavedChanges = true;
        return map;
    }

    public static void SaveJson(EditorMapData map, string path) {
        EditorCatalog.Load();
        var entries = new JsonArray();
        var lookup = new Dictionary<string, ushort>(StringComparer.Ordinal);
        using var raw = new MemoryStream();
        using (var indices = new BinaryWriter(raw, Encoding.UTF8, true)) {
            foreach (var tile in map.Tiles) {
                var entry = BuildJsonTile(tile);
                var key = entry.ToJsonString();
                if (!lookup.TryGetValue(key, out var index)) {
                    if (entries.Count >= ushort.MaxValue) throw new InvalidDataException("Map has too many unique tile combinations.");

                    index = (ushort)entries.Count;
                    lookup[key] = index;
                    entries.Add(entry);
                }

                indices.Write(index);
            }
        }

        raw.Position = 0;
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, true)) raw.CopyTo(zlib);
        var root = new JsonObject {
            ["width"] = map.Width,
            ["height"] = map.Height,
            ["dict"] = entries,
            ["data"] = Convert.ToBase64String(compressed.ToArray()),
        };

        File.WriteAllText(path, root.ToJsonString());
    }

    public static EditorMapData LoadWmap(string path) {
        EditorCatalog.Load();
        using var file = File.OpenRead(path);
        var version = file.ReadByte();
        if (version is < 0 or > 2) throw new InvalidDataException($"Unsupported WMAP version {version}.");

        using var zlib = new ZLibStream(file, CompressionMode.Decompress);
        using var reader = new BinaryReader(zlib, Encoding.UTF8, false);
        var tileCount = reader.ReadUInt16();
        var dictionary = new EditorTileData[tileCount];
        for (var i = 0; i < tileCount; i++) {
            var ground = reader.ReadUInt16();
            var objectId = ReadString(reader);
            var objectConfig = ReadString(reader);
            dictionary[i] = new EditorTileData {
                GroundType = ground == ushort.MaxValue ? -1 : ground,
                ObjectType = EditorCatalog.GetType(EditorDrawType.Objects, objectId),
                ObjectConfig = string.IsNullOrEmpty(objectConfig) ? null : objectConfig,
                TerrainType = reader.ReadByte(),
                RegionType = reader.ReadByte(),
                Elevation = version == 1 ? reader.ReadSByte() : 0,
            };
        }

        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var map = new EditorMapData(Path.GetFileNameWithoutExtension(path), width, height);
        for (var i = 0; i < map.Tiles.Length; i++) {
            var index = reader.ReadUInt16();
            if (index >= dictionary.Length) throw new InvalidDataException("WMAP tile index is outside its dictionary.");

            map.Tiles[i].CopyFrom(dictionary[index]);
            if (version == 2) map.Tiles[i].Elevation = reader.ReadByte();
        }

        map.SavedChanges = true;
        return map;
    }

    public static void SaveWmap(EditorMapData map, string path) {
        EditorCatalog.Load();
        var dictionary = new List<EditorTileData>();
        var lookup = new Dictionary<string, ushort>(StringComparer.Ordinal);
        var indices = new ushort[map.Tiles.Length];
        for (var i = 0; i < map.Tiles.Length; i++) {
            var tile = map.Tiles[i];
            var key = $"{tile.GroundType}|{tile.ObjectType}|{tile.ObjectConfig}|{tile.TerrainType}|{tile.RegionType}|{tile.Elevation}";
            if (!lookup.TryGetValue(key, out var index)) {
                if (dictionary.Count >= ushort.MaxValue) throw new InvalidDataException("Map has too many unique tile combinations.");

                index = (ushort)dictionary.Count;
                lookup[key] = index;
                dictionary.Add(tile.Clone());
            }

            indices[i] = index;
        }

        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, Encoding.UTF8, true)) {
            writer.Write((ushort)dictionary.Count);
            foreach (var tile in dictionary) {
                writer.Write(tile.GroundType < 0 ? ushort.MaxValue : (ushort)tile.GroundType);
                WriteString(writer, EditorCatalog.GetId(EditorDrawType.Objects, tile.ObjectType));
                WriteString(writer, tile.ObjectConfig ?? string.Empty);
                writer.Write((byte)tile.TerrainType);
                writer.Write((byte)tile.RegionType);
                writer.Write((sbyte)tile.Elevation);
            }

            writer.Write(map.Width);
            writer.Write(map.Height);
            foreach (var index in indices) writer.Write(index);
        }

        payload.Position = 0;
        using var file = File.Create(path);
        file.WriteByte(WmapVersion);
        using var zlib = new ZLibStream(file, CompressionLevel.Optimal, false);
        payload.CopyTo(zlib);
    }

    private static void ReadJsonTile(JsonElement entry, EditorTileData tile) {
        if (entry.TryGetProperty("ground", out var ground))
            tile.GroundType = EditorCatalog.GetType(EditorDrawType.Ground, ground.GetString());

        if (entry.TryGetProperty("objs", out var objects) && objects.GetArrayLength() > 0) {
            var obj = objects[objects.GetArrayLength() - 1];
            tile.ObjectType = EditorCatalog.GetType(EditorDrawType.Objects, obj.GetProperty("id").GetString());
            if (obj.TryGetProperty("name", out var name)) tile.ObjectConfig = name.GetString();
        }

        if (entry.TryGetProperty("regions", out var regions) && regions.GetArrayLength() > 0)
            tile.RegionType = EditorCatalog.GetType(EditorDrawType.Regions, regions[0].GetProperty("id").GetString());

        if (entry.TryGetProperty("terrain", out var terrain)) {
            var text = terrain.ValueKind == JsonValueKind.String ? terrain.GetString() : terrain.GetRawText();
            int.TryParse(text, out tile.TerrainType);
        }
    }

    private static int CountValidIndices(byte[] data, int tileCount, int dictionaryCount, bool bigEndian) {
        var valid = 0;
        for (var i = 0; i < tileCount; i++) {
            var bytes = data.AsSpan(i * sizeof(ushort), sizeof(ushort));
            var index = bigEndian
                ? BinaryPrimitives.ReadUInt16BigEndian(bytes)
                : BinaryPrimitives.ReadUInt16LittleEndian(bytes);

            if (index < dictionaryCount) valid++;
        }

        return valid;
    }

    private static JsonObject BuildJsonTile(EditorTileData tile) {
        var entry = new JsonObject();
        if (tile.GroundType != -1) entry["ground"] = EditorCatalog.GetId(EditorDrawType.Ground, tile.GroundType);
        if (tile.ObjectType != 0) {
            var obj = new JsonObject { ["id"] = EditorCatalog.GetId(EditorDrawType.Objects, tile.ObjectType) };
            if (!string.IsNullOrEmpty(tile.ObjectConfig)) obj["name"] = tile.ObjectConfig;
            entry["objs"] = new JsonArray(obj);
        }

        if (tile.RegionType != 0)
            entry["regions"] = new JsonArray(new JsonObject { ["id"] = EditorCatalog.GetId(EditorDrawType.Regions, tile.RegionType) });

        if (tile.TerrainType != 0) entry["terrain"] = tile.TerrainType.ToString();
        return entry;
    }

    private static string ReadString(BinaryReader reader) {
        var length = reader.Read7BitEncodedInt();
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }

    private static void WriteString(BinaryWriter writer, string value) {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        writer.Write7BitEncodedInt(bytes.Length);
        writer.Write(bytes);
    }
}