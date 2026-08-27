using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Alloy.UiLib.Core;

public abstract class DisplayContainer : EventManager {

    internal DisplayContainer() {
    }

    public int NumChildren => _children.Count;

    public Stage Stage { get; internal set; }

    public Sprite Parent { get; private set; }

    internal float SelfContentWidth;

    internal float SelfContentHeight;

    internal int ContentWidth;

    internal int ContentHeight;

    private readonly List<Sprite> _children = [];

    internal ReadOnlySpan<Sprite> GetChildrenSpan() {
        return CollectionsMarshal.AsSpan(_children);
    }

    public T AddChild<T>(T child) where T : Sprite => AddChildAt(child, _children.Count);

    public T AddChildAt<T>(T child, int index) where T : Sprite {
        ValidateChild(child);
        ValidateInsertionIndex(index);

        if (child.Parent == this) {
            var currentIndex = _children.IndexOf(child);
            var targetIndex = Math.Min(index, _children.Count - 1);
            if (currentIndex != targetIndex) {
                _children.RemoveAt(currentIndex);
                _children.Insert(targetIndex, child);
            }

            return child;
        }

        child.Parent?.RemoveChild(child);
        _children.Insert(index, child);
        child.Parent = this as Sprite;

        UpdateBounds();

        var addedToStage = (Stage != null && child.Stage == null);
        if (addedToStage)
            child.SetStageReferenceChildren(Stage);

        child.DispatchEvent(new Event(Event.Added));

        if (addedToStage) {
            var evnt = new Event(Event.AddedToStage);
            child.DispatchEvent(evnt);
            child.DispatchChildren(evnt);
        }

        return child;
    }

    public bool Contains(Sprite child) {
        while (child != this && child != null) {
            child = child.Parent;
        }

        return child == this;
    }

    public Sprite GetChildAt(int index) {
        if (index >= 0 && index < _children.Count)
            return _children[index];

        return null;
    }

    public int GetChildIndex(Sprite child) {
        return _children.IndexOf(child);
    }

    public T RemoveChild<T>(T child) where T : Sprite {
        if (child == null || child.Parent != this)
            return child;

        child.DispatchEvent(new Event(Event.Removed, true));

        if (Stage != null) {
            Stage.ClearFocusWithin(child);
            Stage.ReleasePointersWithin(child);

            var evnt = new Event(Event.RemovedFromStage);
            child.DispatchEvent(evnt);
            child.DispatchChildren(evnt);
            child.SetStageReferenceChildren(null);
        }

        child.Parent = null;
        _children.Remove(child);

        UpdateBounds();

        return child;
    }

    public Sprite RemoveChildAt(int index) {
        if (index >= 0 && index < _children.Count)
            return RemoveChild(_children[index]);

        return null;
    }

    public void RemoveChildren(int start = 0, int end = int.MaxValue) {
        if (end == int.MaxValue) {
            end = _children.Count;
        }

        if (start < 0 || start > _children.Count || end > _children.Count || end < start) {
            throw new Exception("The supplied index is out of bounds");
        }

        var numRemovals = end - start;
        while (numRemovals > 0) {
            RemoveChildAt(start);
            numRemovals--;
        }
    }

    public void SetChildIndex(Sprite child, int index) {
        ValidateChildIndex(index);

        if (child.Parent != this) {
            return;
        }

        _children.Remove(child);
        _children.Insert(index, child);
    }

    public void SwapChildren(Sprite child1, Sprite child2) {
        if (child1.Parent != this || child2.Parent != this) {
            return;
        }

        var idx1 = _children.IndexOf(child1);
        var idx2 = _children.IndexOf(child2);

        _children[idx1] = child2;
        _children[idx2] = child1;
    }

    public void SwapChildrenAt(int index1, int index2) {
        ValidateChildIndex(index1);
        ValidateChildIndex(index2);

        (_children[index1], _children[index2]) = (_children[index2], _children[index1]);
    }

    private void DispatchChildren(Event @event) {
        foreach (var child in _children) {
            if (child.DispatchEvent(@event))
                break;

            child.DispatchChildren(@event);
        }
    }

    private void SetStageReferenceChildren(Stage stage) {
        Stage = stage;
        foreach (var child in _children) {
            child.SetStageReferenceChildren(stage);
        }
    }

    internal void UpdateBounds() {
        var xMin = 0f;
        var xMax = SelfContentWidth;
        var yMin = 0f;
        var yMax = SelfContentHeight;

        foreach (var child in _children) {
            child.GetBoundsInParent(out var childMinX, out var childMinY, out var childMaxX, out var childMaxY);
            xMin = Math.Min(xMin, childMinX);
            xMax = Math.Max(xMax, childMaxX);
            yMin = Math.Min(yMin, childMinY);
            yMax = Math.Max(yMax, childMaxY);
        }

        var newWidth = (int)MathF.Ceiling(xMax - xMin);
        var newHeight = (int)MathF.Ceiling(yMax - yMin);

        if (newWidth == ContentWidth && newHeight == ContentHeight) {
            return;
        }

        ContentWidth = newWidth;
        ContentHeight = newHeight;

        Parent?.UpdateBounds();
    }

    private void ValidateChild(Sprite child) {
        if (child == null) throw new Exception("Tried to add null as child");
        if (child == this) throw new Exception("Tried to add self as child");
        if (child is Stage) throw new Exception("Tried to add stage as child");

        var obj = Parent;
        while (obj != null) {
            if (obj == child) throw new Exception("Tried to add parent as child");

            obj = obj.Parent;
        }
    }

    private void ValidateInsertionIndex(int index) {
        if (index < 0 || index > _children.Count) {
            throw new Exception("Index must be between zero and the number of children");
        }
    }

    private void ValidateChildIndex(int index) {
        if (index < 0 || index >= _children.Count) {
            throw new Exception("Index must reference an existing child position");
        }
    }
}