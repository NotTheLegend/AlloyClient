using System;

namespace AlloyClient.Editor;

public sealed class EditorMapData {
    public string Name;
    public int Width;
    public int Height;
    public EditorTileData[] Tiles;
    public bool SavedChanges;
    public Action Changed;

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
        if (!InBounds(x, y)) return;
        Tiles[x + y * Width].CopyFrom(tile);
        if (notify) MarkChanged();
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
                if (InBounds(nx, ny)) Tiles[nx + ny * Width].CopyFrom(oldTiles[x + y * oldWidth]);
            }
        }
        MarkChanged();
    }

    public void ReplaceAll(string name, int width, int height, EditorTileData[] tiles) {
        Name = name;
        Width = width;
        Height = height;
        Tiles = tiles;
        SavedChanges = true;
        Changed?.Invoke();
    }

    public void MarkChanged() {
        SavedChanges = false;
        Changed?.Invoke();
    }

    private void ResizeStorage(int width, int height) {
        Width = Math.Clamp(width, 1, 2048);
        Height = Math.Clamp(height, 1, 2048);
        Tiles = new EditorTileData[Width * Height];
        for (var i = 0; i < Tiles.Length; i++) Tiles[i] = new EditorTileData();
    }
}

