using System;
using Alloy.Common;
using Alloy.UiLib.Rendering;
using Alloy.UiLib.Utils;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

public abstract class Sprite : DisplayContainer {
    
    private bool _noRenderData = true;
    private Bounds _selfContentBounds = Bounds.Zero;

    protected TextureType TextureId;
    private VertexUi[] _vertexData = [];
    private int _vertexCount;
    
    protected Vector2 Radii;

    private protected sealed override Bounds GetSelfBounds() => _selfContentBounds;

    protected void EnsureBufferLength(int length) {
        if (length % 3 != 0 || length < 0) {
            throw new Exception($"Length needs to be a multiple of 3 and greater than 0, actual: {length}, remainder: {length % 3}");
        }
        
        _vertexCount = length;
        _noRenderData = length == 0;

        if (_vertexData.Length < length) {
            Array.Resize(ref _vertexData, length);
        }
    }
    
    protected Span<VertexUi> GetGraphicsBuffer(int length) {
        EnsureBufferLength(length);
        return _vertexData.AsSpan(0, _vertexCount);
    }

    protected Span<VertexUi> GetGraphicsBuffer() => _vertexData.AsSpan(0, _vertexCount);

    /// <summary>
    /// Only needs to be called if vertex positions change
    /// </summary>
    protected void SetGraphicsBuffer() {
        if (!_noRenderData) {
            var x = 0f;
            var y = 0f;
            var x1 = 0f;
            var y1 = 0f;
            
            foreach (ref var vertex in GetGraphicsBuffer()) {
                x = Math.Min(x, vertex.Position.X);
                y = Math.Min(y, vertex.Position.Y);
                x1 = Math.Max(x1, vertex.Position.X);
                y1 = Math.Max(y1, vertex.Position.Y);
            }
            
            _selfContentBounds = new Bounds((int)x, (int)y, (int)x1, (int)y1);
        } else {
            _selfContentBounds = Bounds.Zero;
        }
        
        DoBoundsUpdate();
    }

    internal sealed override void Draw() {
        if (!Visible) {
            return;
        }

        if (_noRenderData) {
            base.Draw();
            return;
        }
        
        /*if (DirtyInstance) {
            //render.ssbo.subdata(State)
        }*/
        
        var vertexMatrix = new SpriteVertexMatrix(State.Scale, State.Rotation, State.Position, -AnchorOffset);
        var instance = new SpriteInstanceData(vertexMatrix, new Vector2((float) TextureId, State.Alpha), State.Scissor, Extra1, Extra2, ColorTransformation);
        
        SpriteRender.Draw(instance, GetGraphicsBuffer());
        
        UiRender.LastRenderCount++;
        
        base.Draw();
    }

    /// <summary>
    /// Sets color channel 1 in all vertices in current active buffer 
    /// </summary>
    public void SetColorChannel1(Color color) {
        foreach (ref var data in GetGraphicsBuffer()) {
            data.ColorChannel1 = color;
        }
    }
    
    /// <summary>
    /// Sets color channel 2 in all vertices in current active buffer 
    /// </summary>
    public void SetColorChannel2(Color color) {
        foreach (ref var data in GetGraphicsBuffer()) {
            data.ColorChannel2 = color;
        }
    }
    
    protected Vector4 Extra1;
    protected Vector4 Extra2;
    // also rotation
    
    // =========================================

    #region Dragging

    private static Sprite _dragSprite; // move to stage?
    
    public DisplayObject DropTarget { get; private set; }

    public void StartDrag() {
        if (Stage is null) { // flash lets you start a drag with an object not on stage, i'm not going to copy that behavior
            Logger.LogWarning("Aborting drag, sprite is not attached to stage!");
            return;
        }
        
        _dragSprite?.ClearDrag();
        _dragOffset = GetRelativeMousePosition();
        _dragSprite = this;
        _isDragging = true;
    }

    private void ClearDrag() {
        _isDragging = false;
        _dragOffset = Vector2i.Zero;
        _dragSprite = null;
    }

    public void StopDrag() {
        if (_dragSprite is null || Stage is null) {
            return;
        }
        
        DropHitTest();
        ClearDrag();
    }

    private void DropHitTest() {
        DisplayObject target = null;

        var (mouse, children) = (_dragSprite.MouseEnabled, _dragSprite.MouseChildren);
        (_dragSprite.MouseEnabled, _dragSprite.MouseChildren) = (false, false); // remove self from HitTest
        
        Stage.HitTest(Stage.Mouse.GetMousePosition(), ref target);
        
        (_dragSprite.MouseEnabled, _dragSprite.MouseChildren) = (mouse, children);
        
        DropTarget = target;
    }

    #endregion

    #region Hitboxes
    
    private protected sealed override bool HitboxSquare(Vector2i position) => _selfContentBounds.Contains(position);
    
    private protected sealed override bool HitboxEllipse(Vector2i position) {
        var (rx, ry) = Radii;
        var (x, y) = position; // Mouse
        return (x - rx) * (x - rx) / (rx * rx) + (y - ry) * (y - ry) / (ry * ry) <= 1;
    }
    
    private protected sealed override bool HitboxComplex(Vector2i position) {
        if (_noRenderData || _vertexCount == 0) {
            return false;
        }
        
        for (var i = 0; i < _vertexCount; i += 3) {
            var t1 = _vertexData[i + 0].Position;
            var t2 = _vertexData[i + 1].Position;
            var t3 = _vertexData[i + 2].Position;

            var d1 = (position.X - t2.X) * (t1.Y - t2.Y) - (t1.X - t2.X) * (position.Y - t2.Y);
            var d2 = (position.X - t3.X) * (t2.Y - t3.Y) - (t2.X - t3.X) * (position.Y - t3.Y);
            var d3 = (position.X - t1.X) * (t3.Y - t1.Y) - (t3.X - t1.X) * (position.Y - t1.Y);
            
            if (!((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0))) {
                return true;
            }
        }
        
        return false;
    }
    
    #endregion

}