using System;
using Common.Vector;
using MonoClient.UiLib.Input;

namespace MonoClient.UiLib.Core;

public partial class Sprite {
    
    private static Sprite _dragSprite;
    private static Type _dropType;

    public Sprite DropTarget;
    
    private bool _isDragging;
    
    private IntVector2 _dragOffset;

    

    public void StartDrag() => StartDrag<Sprite>();

    public void StartDrag<T>() where T : Sprite {
        if (_dragSprite != null)
            _dragSprite._isDragging = false;
        
        var pos = MouseInput.GetMousePosition();
        pos.X -= _trueX;
        pos.Y -= _trueY;

        _dragOffset = new IntVector2((int)(pos.X / _trueScale.X), (int)(pos.Y / _trueScale.Y));
        
        _dragSprite = this;
        _isDragging = true;
        _dropType = typeof(T);
    }

    public void EndDrag() {
        _isDragging = false;
        GetDragTarget();
    }

    private void GetDragTarget() {
        if (_dragSprite == null) return;
        if (_dragSprite._isDragging) return;

        // get lowest hierarchy sprite
        var current = this;
        var next = this;

        while (next != null) {
            next = next._parent;

            if (next != null)
                current = next;
        }
        
        DropTarget = null;
        var pos = MouseInput.GetMousePosition();
        current.DropCheck(pos, ref DropTarget);
        _dragSprite = null;
        _dropType = null;
    }

    private void DropCheck(IntVector2 pos, ref Sprite target) {
        if (!IsInBounds(pos) || this == _dragSprite)
            return;
        
        if (_dropType.IsInstanceOfType(this))
            target = this;
        
        foreach (var child in _children) {
            child.DropCheck(pos, ref target);
        }
    }

}