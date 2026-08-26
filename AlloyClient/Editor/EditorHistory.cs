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
    public EditorSelection BeforeSelection;
    public EditorSelection AfterSelection;
    public bool SelectionChanged;

    public void Add(int x, int y, EditorTileData before, EditorTileData after) {
        if (!before.SameAs(after)) Changes.Add(new EditorTileChange(x, y, before, after));
    }

    public void SetSelection(EditorSelection before, EditorSelection after) {
        BeforeSelection = before?.Clone();
        AfterSelection = after?.Clone();
        SelectionChanged = !SameSelection(BeforeSelection, AfterSelection);
    }

    public bool IsEmpty() => Changes.Count == 0 && !SelectionChanged;

    private static bool SameSelection(EditorSelection first, EditorSelection second) {
        if (first is null || second is null) return first is null && second is null;

        return first.StartX == second.StartX && first.StartY == second.StartY
                                             && first.EndX == second.EndX && first.EndY == second.EndY;
    }

    public void Undo(EditorMapData map, EditorSelection selection) {
        for (var i = Changes.Count - 1; i >= 0; i--)
            map.SetTile(Changes[i].X, Changes[i].Y, Changes[i].Before, false);

        if (SelectionChanged) selection.CopyFrom(BeforeSelection);
        if (Changes.Count > 0) map.MarkChanged();
    }

    public void Redo(EditorMapData map, EditorSelection selection) {
        for (var i = 0; i < Changes.Count; i++)
            map.SetTile(Changes[i].X, Changes[i].Y, Changes[i].After, false);

        if (SelectionChanged) selection.CopyFrom(AfterSelection);
        if (Changes.Count > 0) map.MarkChanged();
    }
}

public sealed class EditorHistory {
    private readonly List<EditorActionSet> _present = [];
    private readonly List<EditorActionSet> _erased = [];

    public void Record(EditorActionSet actions) {
        if (actions is null || actions.IsEmpty()) return;

        _present.Add(actions);
        _erased.Clear();
    }

    public bool Undo(EditorMapData map, EditorSelection selection) {
        if (_present.Count == 0) return false;

        var index = _present.Count - 1;
        var actions = _present[index];
        _present.RemoveAt(index);
        actions.Undo(map, selection);
        _erased.Add(actions);
        return true;
    }

    public bool Redo(EditorMapData map, EditorSelection selection) {
        if (_erased.Count == 0) return false;

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