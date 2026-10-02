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
    protected VertexUi[] VertexData;
    protected int OverridePrimCount = -1;
    
    protected Vector2 Radii;

    private protected sealed override Bounds GetSelfBounds() => _selfContentBounds;

    internal sealed override void SetStageReference(Stage stage) {
        base.SetStageReference(stage);
    }
    
    protected Span<VertexUi> EnsureBufferCapacity(int length) {
        if (length % 3 != 0) {
            throw new Exception("length needs to be a multiple of 3");
        }

        OverridePrimCount = -1;
        
        if (VertexData is null) {
            VertexData = new VertexUi[length];
            return VertexData;
        }

        if (length < VertexData.Length) {
            OverridePrimCount = length / 3;
            return VertexData.AsSpan(0, length);
        }
        
        Array.Resize(ref VertexData, length);
        return VertexData;
    }
    
    protected void SetGraphicsBuffer() {
        if (VertexData is not null && VertexData.Length % 3 != 0) {
            throw new Exception("VertexData length needs to be a multiple of 3");
        }
        
        _noRenderData = VertexData is null;

        if (!_noRenderData) {
            var x = 0f;
            var y = 0f;
            var x1 = 0f;
            var y1 = 0f;


            var span = OverridePrimCount > 0 ? VertexData.AsSpan(0, OverridePrimCount * 3) : VertexData.AsSpan();
            
            foreach (ref var vertex in span) {
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
        var instance = new SpriteInstanceData(vertexMatrix, Color, ColorSecondary, new Vector2((float) TextureId, State.Alpha), State.Scissor, Extra1, Extra2, ColorTransformation);

        var vCount = OverridePrimCount > 0 ? OverridePrimCount * 3 : VertexData.Length;
        
        SpriteRender.Draw(instance, VertexData.AsSpan(0, vCount));
        
        UiRender.LastRenderCount++;
        
        base.Draw();
    }

    public void SetColorChannel1(Color color) {
        foreach (ref var data in VertexData.AsSpan()) {
            data.Color = color;
        }
    }
    
    
    // do something with
    private Vector2 _info;
    
    public bool TooltipMode; // remove
    public Color Color;
    public Color ColorSecondary;
    protected Vector4 Extra1;
    protected Vector4 Extra2;
    // also rotation
    
    // migrate into vertex data
    public void SetColor(uint rgb, float alpha = 1f) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        var a = (byte)(Math.Max(Math.Min(alpha, 1f), 0f) * byte.MaxValue);
        Color.PackedValue = (uint)(a << 24 | b << 16 | g << 8 | r);
    }
    
    public void SetColorSecondary(uint rgb, float alpha = 1f) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        var a = (byte)(Math.Max(Math.Min(alpha, 1f), 0f) * byte.MaxValue);
        ColorSecondary.PackedValue = (uint)(a << 24 | b << 16 | g << 8 | r);
    }
    
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
        if (_noRenderData || OverridePrimCount == 0) {
            return false;
        }
        
        var len = OverridePrimCount > -1 ? OverridePrimCount * 3 : VertexData.Length;
        for (var i = 0; i < len; i += 3) {
            var t1 = VertexData[i + 0].Position;
            var t2 = VertexData[i + 1].Position;
            var t3 = VertexData[i + 2].Position;

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