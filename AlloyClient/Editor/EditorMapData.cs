using System;

namespace AlloyClient.Editor;

public sealed class EditorMapData {
    public const int GroundRenderChange = 1;
    public const int ObjectRenderChange = 2;

    public string Name;
    public int Width;
    public int Height;
    public EditorTileData[] Tiles;
    public bool SavedChanges;
    public Action Changed;

    private int _renderChanges;
    private int _dirtyMinX;
    private int _dirtyMinY;
    private int _dirtyMaxX;
    private int _dirtyMaxY;

    public EditorMapData(string name, int width, int height) {
        Name = string.IsNullOrWhiteSpace(name) ? "untitled" : name;
        ResizeStorage(width, height);
        SavedChanges = false;
    }

    public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

    public EditorTileData GetTile(int x, int y) {
        return InBounds(x, y) ? Tiles[x + y * Width] : null;
    }

    public void SetTile(int x, int y, EditorTileData tile, bool notify = true) {
        if (!InBounds(x, y)) {
            return;
        }

        var current = Tiles[x + y * Width];
        var changes = 0;
        if (current.GroundType != tile.GroundType
            || current.TerrainType != tile.TerrainType
            || current.Elevation != tile.Elevation) {
            changes |= GroundRenderChange;
        }

        if (current.ObjectType != tile.ObjectType) {
            changes |= ObjectRenderChange;
        }

        current.CopyFrom(tile);
        MarkRenderDirty(x, y, changes);

        if (notify) {
            MarkChanged();
        }
    }

    public void ResizeMap(int width, int height) {
        width = Math.Clamp(width, 1, 2048);
        height = Math.Clamp(height, 1, 2048);
        var oldTiles = Tiles;
        var oldWidth = Width;
        var oldHeight = Height;
        ResizeStorage(width, height);

        var offsetX = (width - oldWidth) / 2;
        var offsetY = (height - oldHeight) / 2;
        for (var y = 0; y < oldHeight; y++) {
            for (var x = 0; x < oldWidth; x++) {
                var nx = x + offsetX;
                var ny = y + offsetY;
                if (InBounds(nx, ny)) {
                    Tiles[nx + ny * Width].CopyFrom(oldTiles[x + y * oldWidth]);
                }
            }
        }

        MarkChanged();
    }

    public void MarkChanged() {
        SavedChanges = false;
        Changed?.Invoke();
    }

    public void ConsumeRenderChanges(out int minX, out int minY, out int maxX, out int maxY, out int changes) {
        changes = _renderChanges;
        minX = _dirtyMinX;
        minY = _dirtyMinY;
        maxX = _dirtyMaxX;
        maxY = _dirtyMaxY;
        _renderChanges = 0;
    }

    private void MarkAllRenderDirty() {
        _renderChanges = GroundRenderChange | ObjectRenderChange;
        _dirtyMinX = 0;
        _dirtyMinY = 0;
        _dirtyMaxX = Width - 1;
        _dirtyMaxY = Height - 1;
    }

    private void MarkRenderDirty(int x, int y, int changes) {
        if (changes == 0) {
            return;
        }

        if (_renderChanges == 0) {
            _dirtyMinX = _dirtyMaxX = x;
            _dirtyMinY = _dirtyMaxY = y;
        } else {
            _dirtyMinX = Math.Min(_dirtyMinX, x);
            _dirtyMinY = Math.Min(_dirtyMinY, y);
            _dirtyMaxX = Math.Max(_dirtyMaxX, x);
            _dirtyMaxY = Math.Max(_dirtyMaxY, y);
        }

        _renderChanges |= changes;
    }

    private void ResizeStorage(int width, int height) {
        Width = Math.Clamp(width, 1, 2048);
        Height = Math.Clamp(height, 1, 2048);
        Tiles = new EditorTileData[Width * Height];

        for (var i = 0; i < Tiles.Length; i++) {
            Tiles[i] = new EditorTileData();
        }

        MarkAllRenderDirty();
    }
}