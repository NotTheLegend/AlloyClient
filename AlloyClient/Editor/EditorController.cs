using System;
using System.Collections.Generic;

namespace AlloyClient.Editor;

public sealed class EditorController {
    public EditorMapData Map;
    public readonly EditorHistory History = new();
    public readonly EditorBrush Brush = new();
    public readonly EditorSelection Selection = new();
    public readonly EditorClipboard Clipboard = new();
    public EditorToolType Tool = EditorToolType.Pencil;
    public Action Changed;
    public Action<string> Status;

    private EditorActionSet _stroke;
    private int _startX;
    private int _startY;
    private int _lastX = int.MinValue;
    private int _lastY = int.MinValue;
    private EditorSelection _selectionBefore;
    private bool _selectionMoving;
    private int _selectionPointerX;
    private int _selectionPointerY;
    private EditorSelection _moveOrigin;
    private EditorTileData[] _movingTiles = [];
    private EditorTileData[] _movingUnder = [];
    private readonly Dictionary<int, EditorTileData> _moveBefore = [];

    public EditorController(EditorMapData map) {
        SetMap(map);
    }

    public void SetMap(EditorMapData map) {
        Map = map;
        History.Clear();
        Selection.Clear();
        Map.Changed = NotifyChanged;
        NotifyChanged();
    }

    public void Begin(int x, int y) {
        if (!Map.InBounds(x, y)) return;
        _startX = x;
        _startY = y;
        _lastX = int.MinValue;
        _lastY = int.MinValue;
        _stroke = new EditorActionSet();
        if (Tool == EditorToolType.Select) _selectionBefore = Selection.Clone();

        switch (Tool) {
            case EditorToolType.Pencil: ApplyBrush(x, y, false, _stroke); break;
            case EditorToolType.Eraser: ApplyBrush(x, y, true, _stroke); break;
            case EditorToolType.Bucket: Fill(x, y, _stroke); break;
            case EditorToolType.Picker: Pick(x, y); break;
            case EditorToolType.Shape: ApplyShape(x, y, _stroke); break;
            case EditorToolType.Edit: Status?.Invoke("Object name editing will open for the selected tile."); break;
        }
    }

    public void Drag(int x, int y) {
        if (!Map.InBounds(x, y) || (x == _lastX && y == _lastY)) return;
        _lastX = x;
        _lastY = y;
        switch (Tool) {
            case EditorToolType.Pencil: ApplyBrush(x, y, false, _stroke); break;
            case EditorToolType.Eraser: ApplyBrush(x, y, true, _stroke); break;
            case EditorToolType.Select: Selection.Set(_startX, _startY, x, y); Changed?.Invoke(); break;
        }
    }

    public void End(int x, int y) {
        if (_stroke is null) return;
        if (Map.InBounds(x, y)) {
            if (Tool == EditorToolType.Line) ApplyLine(_startX, _startY, x, y, _stroke);
            if (Tool == EditorToolType.Select) {
                if (_startX == x && _startY == y && _selectionBefore.IsActive()) Selection.Clear();
                else Selection.Set(_startX, _startY, x, y);
            }
        }
        if (Tool == EditorToolType.Select) _stroke.SetSelection(_selectionBefore, Selection);
        Record(_stroke);
        _stroke = null;
        Changed?.Invoke();
    }

    public void Undo() {
        if (History.Undo(Map, Selection)) Changed?.Invoke();
    }

    public void Redo() {
        if (History.Redo(Map, Selection)) Changed?.Invoke();
    }

    public void Copy() {
        Clipboard.Clear();
        if (!Selection.IsActive()) {
            Status?.Invoke("There is no selection to copy.");
            return;
        }
        Clipboard.Copy(Map, Selection);
        Status?.Invoke($"Copied {Clipboard.Width} x {Clipboard.Height} tiles.");
    }

    public void Paste(int x, int y) {
        if (Clipboard.Tiles.Length == 0) return;
        if (x < 0 || y < 0 || x + Clipboard.Width > Map.Width || y + Clipboard.Height > Map.Height) {
            Status?.Invoke("The copied tiles do not fit at that position.");
            return;
        }
        var actions = new EditorActionSet();
        var previousSelection = Selection.Clone();
        for (var cy = 0; cy < Clipboard.Height; cy++) {
            for (var cx = 0; cx < Clipboard.Width; cx++) {
                var mapX = x + cx;
                var mapY = y + cy;
                Replace(mapX, mapY, Clipboard.GetTile(cx, cy), actions);
            }
        }
        Selection.Set(x, y, x + Clipboard.Width - 1, y + Clipboard.Height - 1);
        actions.SetSelection(previousSelection, Selection);
        Record(actions);
        Status?.Invoke($"Pasted {Clipboard.Width} x {Clipboard.Height} tiles.");
        Changed?.Invoke();
    }

    public void ClearSelection() {
        if (!Selection.IsActive()) return;
        var actions = new EditorActionSet();
        var previous = Selection.Clone();
        Selection.Clear();
        actions.SetSelection(previous, Selection);
        Record(actions);
        Changed?.Invoke();
    }

    public bool BeginSelectionMove(int pointerX, int pointerY) {
        if (Tool != EditorToolType.Select || !Selection.IsActive() || !Selection.Contains(pointerX, pointerY)) return false;
        _selectionMoving = true;
        _selectionPointerX = pointerX;
        _selectionPointerY = pointerY;
        _moveOrigin = Selection.Clone();
        _moveBefore.Clear();
        _movingTiles = CaptureTiles(Selection);
        _movingUnder = CreateEmptyTiles(Selection.GetWidth() * Selection.GetHeight());
        return true;
    }

    public void DragSelectionMove(int pointerX, int pointerY) {
        if (!_selectionMoving) return;
        var startX = _moveOrigin.StartX + pointerX - _selectionPointerX;
        var startY = _moveOrigin.StartY + pointerY - _selectionPointerY;
        var endX = startX + _moveOrigin.GetWidth() - 1;
        var endY = startY + _moveOrigin.GetHeight() - 1;
        if (!Map.InBounds(startX, startY) || !Map.InBounds(endX, endY)
            || startX == Selection.StartX && startY == Selection.StartY) return;

        RestoreMovingUnder();
        var nextUnder = CaptureTiles(startX, startY, endX, endY);
        TrackAreaBefore(startX, startY, endX, endY);
        PasteTiles(_movingTiles, startX, startY, _moveOrigin.GetWidth(), _moveOrigin.GetHeight());
        _movingUnder = nextUnder;
        Selection.Set(startX, startY, endX, endY);
        Changed?.Invoke();
    }

    public void EndSelectionMove() {
        if (!_selectionMoving) return;
        _selectionMoving = false;
        var actions = new EditorActionSet();
        foreach (var pair in _moveBefore) {
            var x = pair.Key % Map.Width;
            var y = pair.Key / Map.Width;
            actions.Add(x, y, pair.Value, Map.GetTile(x, y));
        }
        actions.SetSelection(_moveOrigin, Selection);
        Record(actions);
        _moveBefore.Clear();
        Changed?.Invoke();
    }

    public void DeleteSelection() {
        if (!Selection.IsActive()) return;
        var actions = new EditorActionSet();
        for (var y = Selection.StartY; y <= Selection.EndY; y++)
        for (var x = Selection.StartX; x <= Selection.EndX; x++)
            EraseTile(x, y, actions);
        Record(actions);
        Changed?.Invoke();
    }

    public void MoveSelection(int dx, int dy) {
        if (!Selection.IsActive() || (dx == 0 && dy == 0)) return;
        var pointerX = Selection.StartX;
        var pointerY = Selection.StartY;
        if (!BeginSelectionMove(pointerX, pointerY)) return;
        DragSelectionMove(pointerX + dx, pointerY + dy);
        EndSelectionMove();
    }

    public void SetObjectConfig(int x, int y, string config) {
        var tile = Map.GetTile(x, y);
        if (tile is null || tile.ObjectType == 0) return;
        var next = tile.Clone();
        next.ObjectConfig = string.IsNullOrWhiteSpace(config) ? null : config;
        var actions = new EditorActionSet();
        Replace(x, y, next, actions);
        Record(actions);
        Changed?.Invoke();
    }

    private void ApplyBrush(int centerX, int centerY, bool erase, EditorActionSet actions) {
        var radius = Brush.Size;
        for (var y = centerY - radius; y <= centerY + radius; y++) {
            for (var x = centerX - radius; x <= centerX + radius; x++) {
                var dx = x - centerX;
                var dy = y - centerY;
                if (dx * dx + dy * dy > radius * radius || !Selection.Contains(x, y)) continue;
                if (erase) EraseTile(x, y, actions);
                else PaintTile(x, y, actions);
            }
        }
    }

    private void ApplyShape(int centerX, int centerY, EditorActionSet actions) {
        var radius = Brush.Size;
        if (radius == 0) {
            PaintTile(centerX, centerY, actions);
            return;
        }
        for (var y = centerY - radius; y <= centerY + radius; y++) {
            for (var x = centerX - radius; x <= centerX + radius; x++) {
                var dx = x - centerX;
                var dy = y - centerY;
                if (dx * dx + dy * dy > radius * radius || Random.Shared.Next(100) >= Brush.Chance) continue;
                PaintTile(x, y, actions);
            }
        }
    }

    private void ApplyLine(int x0, int y0, int x1, int y1, EditorActionSet actions) {
        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;
        while (true) {
            ApplyBrush(x0, y0, false, actions);
            if (x0 == x1 && y0 == y1) break;
            var twice = error * 2;
            if (twice >= dy) { error += dy; x0 += sx; }
            if (twice <= dx) { error += dx; y0 += sy; }
        }
    }

    private void Fill(int startX, int startY, EditorActionSet actions) {
        var target = Map.GetTile(startX, startY).GetType(Brush.DrawType);
        var replacement = Brush.GetSelectedType();
        if (target == replacement || replacement == (Brush.DrawType == EditorDrawType.Ground ? -1 : 0)) return;
        var pending = new Stack<(int X, int Y)>();
        var visited = new HashSet<int>();
        pending.Push((startX, startY));
        while (pending.TryPop(out var pos)) {
            if (!Map.InBounds(pos.X, pos.Y) || !Selection.Contains(pos.X, pos.Y)) continue;
            var index = pos.X + pos.Y * Map.Width;
            if (!visited.Add(index) || Map.GetTile(pos.X, pos.Y).GetType(Brush.DrawType) != target) continue;
            PaintTile(pos.X, pos.Y, actions);
            pending.Push((pos.X - 1, pos.Y));
            pending.Push((pos.X + 1, pos.Y));
            pending.Push((pos.X, pos.Y - 1));
            pending.Push((pos.X, pos.Y + 1));
        }
    }

    private void Pick(int x, int y) {
        var tile = Map.GetTile(x, y);
        var type = tile.GetType(Brush.DrawType);
        if (type == (Brush.DrawType == EditorDrawType.Ground ? -1 : 0)) return;
        Brush.SetSelectedType(type);
        Status?.Invoke($"Picked {EditorCatalog.GetId(Brush.DrawType, type)}.");
        Changed?.Invoke();
    }

    private void PaintTile(int x, int y, EditorActionSet actions) {
        var tile = Map.GetTile(x, y);
        if (tile is null) return;
        var selected = Brush.GetSelectedType();
        var empty = Brush.DrawType == EditorDrawType.Ground ? -1 : 0;
        if (selected == empty || tile.GetType(Brush.DrawType) == selected) return;
        if (!Brush.Replace && tile.GetType(Brush.DrawType) != empty) return;
        var next = tile.Clone();
        switch (Brush.DrawType) {
            case EditorDrawType.Ground: next.GroundType = selected; break;
            case EditorDrawType.Objects: next.ObjectType = selected; break;
            case EditorDrawType.Regions: next.RegionType = selected; break;
        }
        Replace(x, y, next, actions);
    }

    private void EraseTile(int x, int y, EditorActionSet actions) {
        var tile = Map.GetTile(x, y);
        if (tile is null) return;
        var next = tile.Clone();
        switch (Brush.DrawType) {
            case EditorDrawType.Ground: next.GroundType = -1; break;
            case EditorDrawType.Objects: next.ObjectType = 0; next.ObjectConfig = null; break;
            case EditorDrawType.Regions: next.RegionType = 0; break;
        }
        Replace(x, y, next, actions);
    }

    private void Replace(int x, int y, EditorTileData next, EditorActionSet actions) {
        var before = Map.GetTile(x, y);
        if (before is null || before.SameAs(next)) return;
        actions.Add(x, y, before, next);
        Map.SetTile(x, y, next, false);
    }

    private void Record(EditorActionSet actions) {
        if (actions is null || actions.IsEmpty()) return;
        History.Record(actions);
        if (actions.Changes.Count > 0) Map.MarkChanged();
    }

    private EditorTileData[] CaptureTiles(EditorSelection selection) {
        return CaptureTiles(selection.StartX, selection.StartY, selection.EndX, selection.EndY);
    }

    private EditorTileData[] CaptureTiles(int startX, int startY, int endX, int endY) {
        var width = endX - startX + 1;
        var height = endY - startY + 1;
        var tiles = new EditorTileData[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            tiles[x + y * width] = Map.GetTile(startX + x, startY + y).Clone();
        return tiles;
    }

    private static EditorTileData[] CreateEmptyTiles(int count) {
        var tiles = new EditorTileData[count];
        for (var i = 0; i < count; i++) tiles[i] = new EditorTileData();
        return tiles;
    }

    private void RestoreMovingUnder() {
        TrackAreaBefore(Selection.StartX, Selection.StartY, Selection.EndX, Selection.EndY);
        PasteTiles(_movingUnder, Selection.StartX, Selection.StartY, Selection.GetWidth(), Selection.GetHeight());
    }

    private void PasteTiles(EditorTileData[] tiles, int startX, int startY, int width, int height) {
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            Map.SetTile(startX + x, startY + y, tiles[x + y * width], false);
    }

    private void TrackAreaBefore(int startX, int startY, int endX, int endY) {
        for (var y = startY; y <= endY; y++) {
            for (var x = startX; x <= endX; x++) {
                var index = x + y * Map.Width;
                if (!_moveBefore.ContainsKey(index)) _moveBefore[index] = Map.GetTile(x, y).Clone();
            }
        }
    }

    private void NotifyChanged() => Changed?.Invoke();
}
