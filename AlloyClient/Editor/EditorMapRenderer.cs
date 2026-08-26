using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Alloy.Common.Structs;
using Alloy.Engine;
using AlloyClient.Assets;
using AlloyClient.Assets.Libraries;
using AlloyClient.Game;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering;
using AlloyClient.Rendering.Types;
using AlloyClient.Rendering.VertexData;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Editor;

public sealed class EditorMapRenderer {
    private readonly List<TileData> _tileData = [];
    private readonly List<Entity> _entities = [];
    private readonly List<VertexObject> _objectData = [];
    private MapTile[] _groundTiles = [];
    private int[] _groundTypes = [];
    private int _width;
    private int _height;

    public void Rebuild(EditorMapData map) {
        EnsureGroundStorage(map.Width, map.Height);
        for (var y = 0; y < map.Height; y++) {
            for (var x = 0; x < map.Width; x++) {
                var index = x + y * map.Width;
                var type = map.Tiles[index].GroundType;
                if (_groundTypes[index] == type) continue;
                _groundTypes[index] = type;
                if (type < 0 || !GroundLibrary.TypeToTextureData.ContainsKey((ushort)type)) {
                    _groundTiles[index] = null;
                    continue;
                }
                var tile = new MapTile(new Vector2i(x, y));
                tile.SetType((ushort)type);
                _groundTiles[index] = tile;
            }
        }

        var neighbors = new MapTile[9];
        _tileData.Clear();
        for (var y = 0; y < map.Height; y++) {
            for (var x = 0; x < map.Width; x++) {
                var tile = _groundTiles[x + y * map.Width];
                if (tile is null) continue;
                var neighborIndex = 0;
                for (var ny = y - 1; ny <= y + 1; ny++) {
                    for (var nx = x - 1; nx <= x + 1; nx++) {
                        neighbors[neighborIndex++] = nx >= 0 && ny >= 0 && nx < map.Width && ny < map.Height
                            ? _groundTiles[nx + ny * map.Width]
                            : null;
                    }
                }
                tile.Rebuild(neighbors);
                _tileData.AddRange(tile.DrawTile());
            }
        }
        RebuildObjects(map);
    }

    public void Draw(GameTime gameTime, Camera camera) {
        Render.SetShaderParams(gameTime, camera);
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        var tiles = CollectionsMarshal.AsSpan(_tileData);
        while (tiles.Length > 0) {
            var count = Math.Min(tiles.Length, Render.TileBufferSize);
            Render.DrawTiles(tiles[..count]);
            tiles = tiles[count..];
        }

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        _objectData.Clear();
        Render.StartDrawModel();
        for (var typeIndex = 0; typeIndex < (int)ModelType.Count; typeIndex++) {
            var modelType = (ModelType)typeIndex;
            if (!ModelData.ModelRenderInfo.ContainsKey(modelType)) continue;
            Render.SetEntityModel(modelType);
            foreach (var entity in _entities) {
                var render = entity.RenderBaseType;
                if (render.ModelType == modelType) render.Draw(_objectData, gameTime.TotalMs);
                if (render is TypeWall wall && wall.Top.ModelType == modelType)
                    wall.Top.Draw(_objectData, gameTime.TotalMs);
            }
            Render.FlushBufferModel();
        }

        GL.Disable(EnableCap.CullFace);
        Render.StartDrawEntity();
        Render.FlushBufferEntity(_objectData);
        GL.Enable(EnableCap.CullFace);
    }

    private void EnsureGroundStorage(int width, int height) {
        if (_width == width && _height == height) return;
        _width = width;
        _height = height;
        _groundTiles = new MapTile[width * height];
        _groundTypes = new int[width * height];
        Array.Fill(_groundTypes, int.MinValue);
    }

    private void RebuildObjects(EditorMapData map) {
        _entities.Clear();
        for (var y = 0; y < map.Height; y++) {
            for (var x = 0; x < map.Width; x++) {
                var type = map.Tiles[x + y * map.Width].ObjectType;
                if (type <= 0) continue;
                if (!ObjectLibrary.TypeToTextureData.ContainsKey((ushort)type)
                    || !ObjectLibrary.TypeToObjectProps.TryGetValue((ushort)type, out var properties)) {
                    continue;
                }
                var entity = new Entity { Properties = properties };
                entity.SetType((ushort)type);
                entity.SetPos(x + 0.5f, y + 0.5f);
                if (entity.RenderBaseType is TypeGameObject or TypePlayer) {
                    entity.RenderBaseType.Scale.Z = 0f;
                    entity.RenderBaseType.Scale.W = 0f;
                }
                entity.RenderBaseType.SetVisibility(true);
                entity.RenderBaseType.SetDepth(0.5f + y / (float)Math.Max(1, map.Height) * 0.4f);
                _entities.Add(entity);
            }
        }
    }
}
