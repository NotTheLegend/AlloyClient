using System.Collections.Generic;

namespace AlloyClient.Editor;

public sealed class EditorTileChange(int x, int y, EditorTileData before, EditorTileData after) {
    public readonly int X = x;
    public readonly int Y = y;
    public readonly EditorTileData Before = before.Clone();
    public readonly EditorTileData After = after.Clone();

}

public sealed class EditorActionSet {
    public readonly List<EditorTileChange> Changes = [];
    private EditorSelection _beforeSelection;
    private EditorSelection _afterSelection;
    private bool _selectionChanged;

    public void Add(int x, int y, EditorTileData before, EditorTileData after) {
        if (!before.SameAs(after)) {
            Changes.Add(new EditorTileChange(x, y, before, after));
        }
    }

    public void SetSelection(EditorSelection before, EditorSelection after) {
        _beforeSelection = before?.Clone();
        _afterSelection = after?.Clone();
        _selectionChanged = !SameSelection(_beforeSelection, _afterSelection);
    }

    public bool IsEmpty() => Changes.Count == 0 && !_selectionChanged;

    private static bool SameSelection(EditorSelection first, EditorSelection second) {
        if (first is null || second is null) {
            return first is null && second is null;
        }

        return first.StartX == second.StartX && first.StartY == second.StartY
                                             && first.EndX == second.EndX && first.EndY == second.EndY;
    }

    public void Undo(EditorMapData map, EditorSelection selection) {
        for (var i = Changes.Count - 1; i >= 0; i--) {
            map.SetTile(Changes[i].X, Changes[i].Y, Changes[i].Before, false);
        }

        if (_selectionChanged) {
            selection.CopyFrom(_beforeSelection);
        }
        
        if (Changes.Count > 0) {
            map.MarkChanged();
        }
    }

    public void Redo(EditorMapData map, EditorSelection selection) {
        foreach (var change in Changes) {
            map.SetTile(change.X, change.Y, change.After, false);
        }

        if (_selectionChanged) {
            selection.CopyFrom(_afterSelection);
        }
        
        if (Changes.Count > 0) {
            map.MarkChanged();
        }
    }
}

public sealed class EditorHistory {
    private readonly List<EditorActionSet> _present = [];
    private readonly List<EditorActionSet> _erased = [];

    public void Record(EditorActionSet actions) {
        if (actions is null || actions.IsEmpty()) {
            return;
        }

        _present.Add(actions);
        _erased.Clear();
    }

    public bool Undo(EditorMapData map, EditorSelection selection) {
        if (_present.Count == 0) {
            return false;
        }

        var index = _present.Count - 1;
        var actions = _present[index];
        _present.RemoveAt(index);
        actions.Undo(map, selection);
        _erased.Add(actions);
        return true;
    }

    public bool Redo(EditorMapData map, EditorSelection selection) {
        if (_erased.Count == 0) {
            return false;
        }

        var index = _erased.Count - 1;
        var actions = _erased[index];
        _erased.RemoveAt(index);
        actions.Redo(map, selection);
        _present.Add(actions);
        return true;
    }

    public void Clear() {
        _present.Clear();
        _erased.Clear();
    }
}