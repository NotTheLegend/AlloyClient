using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    private readonly List<TileData> _visibleTileData = [];
    private readonly List<VertexObject> _objectData = [];
    private readonly List<RenderBase>[] _rendersByModel = CreateRenderBuckets();
    private MapTile[] _groundTiles = [];
    private int[] _groundTypes = [];
    private RenderBase[] _primaryObjectRenders = [];
    private RenderBase[] _secondaryObjectRenders = [];
    private EditorMapData _map;
    private int _width;
    private int _height;
    private int _visibleFirstX = -1;
    private int _visibleFirstY = -1;
    private int _visibleLastX = -1;
    private int _visibleLastY = -1;
    private bool _visibleTilesDirty = true;
    private int _tileUploadVersion = -1;

    public void Rebuild(EditorMapData map) {
        var fullRebuild = !ReferenceEquals(_map, map) || _width != map.Width || _height != map.Height;
        map.ConsumeRenderChanges(out var minX, out var minY, out var maxX, out var maxY, out var changes);
        if (fullRebuild) {
            _map = map;
            EnsureStorage(map.Width, map.Height, true);
            minX = 0;
            minY = 0;
            maxX = map.Width - 1;
            maxY = map.Height - 1;
            changes = EditorMapData.GroundRenderChange | EditorMapData.ObjectRenderChange;
        }

        if (changes == 0) return;

        if ((changes & EditorMapData.GroundRenderChange) != 0) {
            UpdateGroundTypes(map, minX, minY, maxX, maxY);
            RebuildGroundRegion(map, minX - 1, minY - 1, maxX + 1, maxY + 1);
            _visibleTilesDirty = true;
        }

        if ((changes & EditorMapData.ObjectRenderChange) != 0)
            RebuildObjectRegion(map, minX, minY, maxX, maxY, fullRebuild);
    }

    public void Draw(GameTime gameTime, Camera camera) {
        Render.SetShaderParams(gameTime, camera);
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        var tileBatchChanged = BuildVisibleTileData(camera);
        var tiles = CollectionsMarshal.AsSpan(_visibleTileData);
        if (tiles.Length <= Render.TileBufferSize) {
            if (tileBatchChanged || _tileUploadVersion != Render.TileUploadVersion) {
                Render.UploadTiles(tiles);
                _tileUploadVersion = Render.TileUploadVersion;
            }

            Render.DrawUploadedTiles(tiles.Length);
        } else {
            while (tiles.Length > 0) {
                var count = Math.Min(tiles.Length, Render.TileBufferSize);
                Render.DrawTiles(tiles[..count]);
                tiles = tiles[count..];
            }

            _tileUploadVersion = -1;
        }

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        _objectData.Clear();
        Render.StartDrawModel();
        for (var typeIndex = 0; typeIndex < (int)ModelType.Count; typeIndex++) {
            var modelType = (ModelType)typeIndex;
            if (!ModelData.ModelRenderInfo.ContainsKey(modelType)) continue;

            Render.SetEntityModel(modelType);
            foreach (var render in _rendersByModel[typeIndex])
                if (IsVisible(render, camera))
                    render.Draw(_objectData, gameTime.TotalMs);

            Render.FlushBufferModel();
        }

        GL.Disable(EnableCap.CullFace);
        Render.StartDrawEntity();
        Render.FlushBufferEntity(_objectData);
        GL.Enable(EnableCap.CullFace);
    }

    private void EnsureStorage(int width, int height, bool clear) {
        if (_width == width && _height == height) {
            if (!clear) return;

            Array.Clear(_groundTiles);
            Array.Fill(_groundTypes, int.MinValue);
            Array.Clear(_primaryObjectRenders);
            Array.Clear(_secondaryObjectRenders);
            return;
        }

        _width = width;
        _height = height;
        _groundTiles = new MapTile[width * height];
        _groundTypes = new int[width * height];
        _primaryObjectRenders = new RenderBase[width * height];
        _secondaryObjectRenders = new RenderBase[width * height];
        Array.Fill(_groundTypes, int.MinValue);
    }

    private void UpdateGroundTypes(EditorMapData map, int minX, int minY, int maxX, int maxY) {
        minX = Math.Max(0, minX);
        minY = Math.Max(0, minY);
        maxX = Math.Min(map.Width - 1, maxX);
        maxY = Math.Min(map.Height - 1, maxY);
        for (var y = minY; y <= maxY; y++)
        for (var x = minX; x <= maxX; x++) {
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

    private void RebuildGroundRegion(EditorMapData map, int minX, int minY, int maxX, int maxY) {
        minX = Math.Max(0, minX);
        minY = Math.Max(0, minY);
        maxX = Math.Min(map.Width - 1, maxX);
        maxY = Math.Min(map.Height - 1, maxY);
        var neighbors = new MapTile[9];
        for (var y = minY; y <= maxY; y++)
        for (var x = minX; x <= maxX; x++) {
            var tile = _groundTiles[x + y * map.Width];
            if (tile is null) continue;

            var neighborIndex = 0;
            for (var ny = y - 1; ny <= y + 1; ny++)
            for (var nx = x - 1; nx <= x + 1; nx++)
                neighbors[neighborIndex++] = nx >= 0 && ny >= 0 && nx < map.Width && ny < map.Height
                    ? _groundTiles[nx + ny * map.Width]
                    : null;

            tile.Rebuild(neighbors);
        }
    }

    private void RebuildObjectRegion(EditorMapData map, int minX, int minY, int maxX, int maxY, bool fullRebuild) {
        if (fullRebuild)
            foreach (var renders in _rendersByModel)
                renders.Clear();

        minX = Math.Max(0, minX);
        minY = Math.Max(0, minY);
        maxX = Math.Min(map.Width - 1, maxX);
        maxY = Math.Min(map.Height - 1, maxY);
        for (var y = minY; y <= maxY; y++)
        for (var x = minX; x <= maxX; x++) {
            var index = x + y * map.Width;
            if (!fullRebuild) {
                RemoveRender(_primaryObjectRenders[index]);
                RemoveRender(_secondaryObjectRenders[index]);
            }

            _primaryObjectRenders[index] = null;
            _secondaryObjectRenders[index] = null;
            var type = map.Tiles[index].ObjectType;
            if (type <= 0
                || !ObjectLibrary.TypeToTextureData.ContainsKey((ushort)type)
                || !ObjectLibrary.TypeToObjectProps.TryGetValue((ushort)type, out var properties)) continue;

            var entity = new Entity { Properties = properties };
            entity.SetType((ushort)type);
            entity.SetPos(x + 0.5f, y + 0.5f);
            if (entity.RenderBaseType is TypeGameObject or TypePlayer) {
                entity.RenderBaseType.Scale.Z = 0f;
                entity.RenderBaseType.Scale.W = 0f;
            }

            entity.RenderBaseType.SetVisibility(true);
            entity.RenderBaseType.SetDepth(0.5f + y / (float)Math.Max(1, map.Height) * 0.4f);
            _primaryObjectRenders[index] = entity.RenderBaseType;
            AddRender(entity.RenderBaseType);
            if (entity.RenderBaseType is not TypeWall wall) continue;

            _secondaryObjectRenders[index] = wall.Top;
            AddRender(wall.Top);
        }
    }

    private bool BuildVisibleTileData(Camera camera) {
        const float edgeMargin = 2f;
        var neededFirstX = Math.Max(0, (int)MathF.Floor(camera.Position.X - camera.VisibleTileRadius.X - edgeMargin));
        var neededFirstY = Math.Max(0, (int)MathF.Floor(camera.Position.Y - camera.VisibleTileRadius.Y - edgeMargin));
        var neededLastX = Math.Min(_width - 1, (int)MathF.Ceiling(camera.Position.X + camera.VisibleTileRadius.X + edgeMargin));
        var neededLastY = Math.Min(_height - 1, (int)MathF.Ceiling(camera.Position.Y + camera.VisibleTileRadius.Y + edgeMargin));
        if (!_visibleTilesDirty
            && neededFirstX >= _visibleFirstX && neededFirstY >= _visibleFirstY
            && neededLastX <= _visibleLastX && neededLastY <= _visibleLastY) {
            return false;
        }

        var paddingX = Math.Max(8, (int)MathF.Ceiling(camera.VisibleTileRadius.X * 0.25f));
        var paddingY = Math.Max(8, (int)MathF.Ceiling(camera.VisibleTileRadius.Y * 0.25f));
        var firstX = Math.Max(0, neededFirstX - paddingX);
        var firstY = Math.Max(0, neededFirstY - paddingY);
        var lastX = Math.Min(_width - 1, neededLastX + paddingX);
        var lastY = Math.Min(_height - 1, neededLastY + paddingY);

        _visibleFirstX = firstX;
        _visibleFirstY = firstY;
        _visibleLastX = lastX;
        _visibleLastY = lastY;
        _visibleTilesDirty = false;
        _visibleTileData.Clear();
        AppendTileData(firstX, firstY, lastX, lastY);
        if (_visibleTileData.Count > Render.TileBufferSize
            && (firstX != neededFirstX || firstY != neededFirstY || lastX != neededLastX || lastY != neededLastY)) {
            _visibleFirstX = neededFirstX;
            _visibleFirstY = neededFirstY;
            _visibleLastX = neededLastX;
            _visibleLastY = neededLastY;
            _visibleTileData.Clear();
            AppendTileData(neededFirstX, neededFirstY, neededLastX, neededLastY);
        }

        return true;
    }

    private void AppendTileData(int firstX, int firstY, int lastX, int lastY) {
        for (var y = firstY; y <= lastY; y++)
        for (var x = firstX; x <= lastX; x++) {
            var tile = _groundTiles[x + y * _width];
            if (tile is not null) _visibleTileData.AddRange(tile.DrawTile());
        }
    }

    private void AddRender(RenderBase render) {
        var index = (int)render.ModelType;
        if (index >= 0 && index < _rendersByModel.Length) _rendersByModel[index].Add(render);
    }

    private void RemoveRender(RenderBase render) {
        if (render is null) return;

        var index = (int)render.ModelType;
        if (index >= 0 && index < _rendersByModel.Length) _rendersByModel[index].Remove(render);
    }

    private static bool IsVisible(RenderBase render, Camera camera) {
        const float margin = 4f;
        return MathF.Abs(render.Position.X - camera.Position.X) <= camera.VisibleTileRadius.X + margin
               && MathF.Abs(render.Position.Y - camera.Position.Y) <= camera.VisibleTileRadius.Y + margin;
    }

    private static List<RenderBase>[] CreateRenderBuckets() {
        var buckets = new List<RenderBase>[(int)ModelType.Count];
        for (var i = 0; i < buckets.Length; i++) buckets[i] = [];
        return buckets;
    }
}