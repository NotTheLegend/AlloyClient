using System;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

public partial class Sprite {
    // TODO: add stage checks to this, cant drag whats not in the display list

    private static Sprite _dragSprite;
    private static Type _dropType;

    public Sprite DropTarget;

    private bool _isDragging;

    private Vector2i _dragOffset;

    public void StartDrag() => StartDrag<Sprite>();

    public void StartDrag<T>() where T : Sprite {
        if (_dragSprite != null)
            _dragSprite._isDragging = false;

        var pos = GlobalToLocal(Stage.Mouse.GetMousePosition());
        _dragOffset = new Vector2i((int)pos.X, (int)pos.Y);

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
            next = next.Parent;

            if (next != null)
                current = next;
        }

        DropTarget = null;
        var pos = Stage.Mouse.GetMousePosition();
        current.DropCheck(pos, ref DropTarget);
        _dragSprite = null;
        _dropType = null;
    }

    private void DropCheck(Vector2i pos, ref Sprite target) {
        if (!Visible || !IsInBounds(pos) || this == _dragSprite)
            return;

        if (_dropType.IsInstanceOfType(this))
            target = this;

        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.DropCheck(pos, ref target);
        }
    }

}