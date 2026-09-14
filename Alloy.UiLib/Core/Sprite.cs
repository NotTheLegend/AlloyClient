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
    protected int OverridePrimCount = -1; // TODO: make private, force EnsureBufferCapacity() usage
    
    protected Vector2 Radii;

    private protected sealed override Bounds GetSelfBounds() => _selfContentBounds;

    internal sealed override void SetStageReference(Stage stage) {
        base.SetStageReference(stage);
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
            
            foreach (ref var vertex in VertexData!.AsSpan()) {
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

        if (DirtyInstance) {
            //render.ssbo.subdata(State)
        }
        // TODO: anchor is dead reference, its built into position already
        var vertexMatrix = new SpriteVertexMatrix(State.Scale, 0f, State.Position, new Vector2(0, 0));
        var instance = new SpriteInstanceData(vertexMatrix, Color, ColorSecondary, new Vector2((float) TextureId, Alpha), _scissor, Extra1, Extra2, ColorTransformation);

        var vCount = OverridePrimCount > 0 ? OverridePrimCount * 3 : VertexData.Length;
        
        SpriteRender.Draw(instance, VertexData.AsSpan(0, vCount));
        
        UiRender.LastRenderCount++;
        
        base.Draw();
    }
    
    
    // do something with
    private Vector2 _info;
    private Vector4 _scissor = new Vector4(0, 0, 10000, 10000);
    
    
    
    
    public bool TooltipMode;
    public Color Color;
    public Color ColorSecondary;
    protected Vector4 Extra1;
    protected Vector4 Extra2;
    
    
    public Vector2i GetRelativeMousePosition() => Vector2i.Zero;
    public void StartDrag() { }
    public void EndDrag() {}
    public Sprite DropTarget;
    
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

    protected void EnsureBufferCapacity(int length) {
        if (length % 3 != 0) {
            throw new Exception("length needs to be a multiple of 3");
        }

        OverridePrimCount = -1;
        
        if (VertexData is null) {
            VertexData = new VertexUi[length];
            return;
        }

        if (length < VertexData.Length) {
            OverridePrimCount = length / 3;
            return;
        }
        
        Array.Resize(ref VertexData, length);
    }
    
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
}