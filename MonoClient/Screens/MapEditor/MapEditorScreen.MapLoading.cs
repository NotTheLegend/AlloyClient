using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.Assets.Libraries;
using MonoClient.Objects;
using MonoClient.Screens.MapEditor.Misc;
using MonoGame.Framework.Utilities.Deflate;
using Newtonsoft.Json;

namespace MonoClient.Screens.MapEditor;

public partial class MapEditorScreen {
    private void CreateNewMap(string name, MapEditorUtils.MapSize mapSize) {
        var mapSizeInt = (int) mapSize;
        var map = new MapStructure {
            Name = name,
            Width = mapSizeInt,
            Height = mapSizeInt,
            Tiles = new MapTile[mapSizeInt, mapSizeInt],
            Objects = new Entity[mapSizeInt, mapSizeInt]
        };

        for (var x = 0; x < mapSizeInt; x++) {
            for (var y = 0; y < mapSizeInt; y++) {
                var tile = new MapTile(x, y);
                tile.SetType(0xFF);
                map.Tiles[x, y] = tile;
            }
        }

        _cameraPosition = new Vector2(mapSizeInt / 2f, mapSizeInt / 2f);

        _maps.Add(map);
        
        Log.Info($"Created new map '{name}' with size {mapSizeInt}x{mapSizeInt}");
    }

    private void LoadMap(string path) {
        var file = File.ReadAllText(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var mapData = JsonConvert.DeserializeObject<JsonData>(file);
        var data = ZlibStream.UncompressBuffer(mapData.data);
        var map = new MapStructure {
            Name = name,
            Width = mapData.width,
            Height = mapData.height,
            Tiles = new MapTile[mapData.width, mapData.height],
            Objects = new Entity[mapData.width, mapData.height]
        };

        using var reader = new BinaryReader(new MemoryStream(data));
        for (var y = 0; y < mapData.height; y++) {
            for (var x = 0; x < mapData.width; x++) {
                var index = reader.ReadInt16();
                var tile = mapData.dict[index];
                ushort tileType;
                if (tile.ground == null) {
                    tileType = 0xFF;
                }
                else if (GroundLibrary.IdToTileType.TryGetValue(tile.ground, out var type)) {
                    tileType = type;
                }
                else {
                    Log.Warn($"Tile: {tile.ground} not found.");
                    continue;
                }

                var mapTile = new MapTile(x, y, true);
                mapTile.SetType(tileType);
                map.Tiles[x, y] = mapTile;

                if (tile.objs is { Length: > 0 }) {
                    var obj = tile.objs[0];
                    var objType = ObjectLibrary.IdToObjectType[obj.id];
                    var mapObj = new Entity {
                        Properties = ObjectLibrary.TypeToObjectProps[objType]
                    };
                    mapObj.SetType(objType);
                    mapObj.SetPos(x, y);
                    mapTile.OccupiedObject = mapObj;
                    map.Objects[x, y] = mapObj;
                    map.EntityStorage.Add(mapObj);
                }

                if (tile.regions is { Length: > 0 }) {
                    var region = tile.regions[0];

                }
            }
        }

        _maps.Add(map);

        _cameraPosition = new Vector2(map.Width / 2f, map.Height / 2f);
        
        map.MapBorderVertices = SetMapBorderVertices(map);
        
        Log.Info($"Loaded map: {name} with size {map.Width}x{map.Height}");
    }
    
    private static VertexPositionColor[] SetMapBorderVertices(MapStructure map) {
        var vertices = new VertexPositionColor[5];
        var color = Color.Blue;
        vertices[0] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0), color);
        vertices[1] = new VertexPositionColor(new Vector3(map.Width + 0.5f, -0.5f, 0), color);
        vertices[2] = new VertexPositionColor(new Vector3(map.Width + 0.5f, map.Height + 0.5f, 0), color);
        vertices[3] = new VertexPositionColor(new Vector3(-0.5f, map.Height + 0.5f, 0), color);
        vertices[4] = vertices[0];
        return vertices;
    }
}