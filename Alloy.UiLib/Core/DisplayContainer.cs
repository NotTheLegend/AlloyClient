using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

public abstract class DisplayContainer : DisplayObject {
    
    
    public bool EnableClipRect; // TODO
    

    public int NumChildren => _children.Count;

    public bool MouseChildren = false;
    
    private readonly List<DisplayObject> _children = [];

    public T AddChild<T>(T child) where T : DisplayObject => AddChildAt(child, _children.Count);

    public T AddChildAt<T>(T child, int index) where T : DisplayObject {
        ValidateChild(child);
        BoundsCheck(index);

        if (child.Parent == this) {
            if (_children[index - 1] != child) {
                _children.Remove(child);
                _children.Insert(index - 1, child);
            }
            return child;
        }

        child.Parent?.RemoveChild(child);
        _children.Insert(index, child);
        child.Parent = this;

        DoBoundsUpdate();

        var addedToStage = (Stage != null && child.Stage == null);
        if (addedToStage) {
            child.SetStageReference(Stage);
        }
        
        child.DispatchEvent(new Event(Event.Added));

        if (addedToStage) {
            var evnt = new Event(Event.AddedToStage);
            child.DispatchWithCapture(evnt);
            DispatchChildren(evnt);
        }
        
        return child;
    }

    public bool Contains(DisplayObject child) {
        while (child != null && child != this) {
            child = child.Parent;

            if (child == this) {
                return true;
            }
        }

        return false;
    }

    public DisplayObject GetChildAt(int index) => index >= 0 && index < _children.Count ? _children[index] : null;

    public int GetChildIndex(DisplayObject child) => _children.IndexOf(child);

    public T RemoveChild<T>(T child) where T : DisplayObject {
        if (child == null || child.Parent != this) {
            return child;
        }
        
        DispatchEvent(new Event(Event.Removed));

        if (Stage != null) {
            var evnt = new Event(Event.RemovedFromStage);
            child.DispatchWithCapture(evnt);
            DispatchChildren(evnt);
            child.SetStageReference(null);
        }

        child.Parent = null;
        _children.Remove(child);
        
        DoBoundsUpdate();

        return child;
    }

    public DisplayObject RemoveChildAt(int index) {
        if (index >= 0 && index < _children.Count)
            return RemoveChild(_children[index]);
        return null;
    }

    public void RemoveChildren(int start = 0, int end = int.MaxValue) {
        if (end == int.MaxValue) end = _children.Count;
        if (start < 0 || end > _children.Count || end < start) throw new Exception("The supplied index is out of bounds");

        var numRemovals = end - start;
        while (numRemovals >= 0)
        {
            RemoveChildAt(start);
            numRemovals--;
        }
    }

    public void SetChildIndex(DisplayObject child, int index) {
        BoundsCheck(index);
        if (child.Parent != this) 
            return;

        _children.Remove(child);
        _children.Insert(index, child);
    }

    public void SwapChildren(DisplayObject child1, DisplayObject child2) {
        if (child1.Parent != this || child2.Parent != this)
            return;

        var idx1 = _children.IndexOf(child1);
        var idx2 = _children.IndexOf(child2);

        _children[idx1] = child2;
        _children[idx2] = child1;
    }

    public void SwapChildrenAt(int index1, int index2) {
        BoundsCheck(index1);
        BoundsCheck(index2);
        
        (_children[index1], _children[index2]) = (_children[index2], _children[index1]);
    }

    internal sealed override void DispatchChildren(Event @event) {
        foreach (var child in _children) {
            @event.SetTarget(child);
            child.DispatchWithCapture(@event);
            child.DispatchChildren(@event);
        }
    }

    internal override void SetStageReference(Stage stage) {
        base.SetStageReference(stage);
        foreach (var child in _children) {
            child.SetStageReference(stage);
        }
    }

    private protected sealed override void DoBoundsUpdate() {
        var bounds = GetSelfBounds();

        foreach (var child in _children) {
            bounds.Merge(child.GetContentBounds());
        }

        if (bounds == ContentBounds) {
            return;
        }

        ContentBounds = bounds;
        Parent?.DoBoundsUpdate();
    }

    private void ValidateChild(DisplayObject child) {
        if (child == null) throw new Exception("Tried to add null as child");
        if (child == this) throw new Exception("Tried to add self as child");
        if (child is Stage) throw new Exception("Tried to add stage as child");
        
        var obj = Parent;
        while (obj != null) {
            if (obj == child) throw new Exception("Tried to add parent as child");
            obj = obj.Parent;
        }
    }

    private void BoundsCheck(int index) {
        if (index < 0) throw new Exception("Index can not be less than 0");
        if (index > _children.Count) throw new Exception("Index can not be greater than number of children");
    }

    internal sealed override void Update(bool dirty, ObjectState state) {
        base.Update(dirty, state);

        foreach (var child in _children) {
            child.Update(DirtyInstance, State);
        }
    }

    internal override void Draw() {
        foreach (var child in _children) {
            child.Draw();
        }
    }

    internal sealed override bool IsInBounds(Vector2i position) {
        if (base.IsInBounds(position)) {
            return true;
        }

        foreach (var child in _children) {
            if (child.IsInBounds(position)) {
                return true;
            }
        }
        
        return false;
    }
}