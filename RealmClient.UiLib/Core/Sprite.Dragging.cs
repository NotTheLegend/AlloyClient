using System;
using OpenTK.Mathematics;
using RealmClient.UiLib.Input;

namespace RealmClient.UiLib.Core;

public partial class Sprite {
    
    private static Sprite _dragSprite;
    private static Type _dropType;
    private static bool _lockMouse;

    public Sprite DropTarget;
    
    private bool _isDragging;
    
    private Vector2i _dragOffset;

    

    public void StartDrag(bool lockMouse = false) => StartDrag<Sprite>(lockMouse);

    public void StartDrag<T>(bool lockMouse = false) where T : Sprite {
        if (_dragSprite != null)
            _dragSprite._isDragging = false;

        _lockMouse = lockMouse;
        
        var pos = MouseInput.GetMousePosition();
        pos.X -= _trueX;
        pos.Y -= _trueY;

        _dragOffset = new Vector2i((int)(pos.X / _trueScale.X), (int)(pos.Y / _trueScale.Y));
        
        _dragSprite = this;
        _isDragging = true;
        _dropType = typeof(T);
    }

    public void EndDrag() {
        _isDragging = false;
        _lockMouse = false;
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

    private void DropCheck(Vector2i pos, ref Sprite target) {
        if (!Visible || !IsInBounds(pos) || this == _dragSprite)
            return;
        
        if (_dropType.IsInstanceOfType(this))
            target = this;
        
        foreach (var child in _children) {
            child.DropCheck(pos, ref target);
        }
    }

}