using System;
using OpenTK.Mathematics;
using RealmClient.UiLib.Rendering;

namespace RealmClient.UiLib.Core;

public partial class Sprite {
    
    private bool _noRenderData = true;
    
    protected virtual void ResizeBackBuffer() {
        if (Indices?.Length is > 0 and < 3) throw new Exception("Primitives require at least 3 indices");
        if (Indices is not null && Indices.Length % 3 != 0) throw new Exception("Total indices not divisible by 3");
        if (VertexData?.Length is > 0 and < 3) throw new Exception("Primitives require at least 3 vertices");
        
        _noRenderData = VertexData is null || Indices is null || VertexData.Length < 3 || Indices.Length < 3;
    }
    
    internal void InternalDrawLoop() {
        SpriteRender.StartDraw();
        Draw();
        SpriteRender.EndDraw();
    }
    
    private void Draw() {
        if (!Visible) return;
        
        if (!_noRenderData && _trueAlpha != 0f && OverridePrimCount != 0)
            DrawInternal();
        
        foreach (var child in _children) {
            child.Draw();
        }
    }
    
    private void DrawInternal() {
        var vertexMatrix = new SpriteVertexMatrix(_trueScale, _trueRotation, new Vector2(_trueX, _trueY), new Vector2(_anchorX, _anchorY));
        var instance = new SpriteInstanceData(vertexMatrix, Color, ColorSecondary, _info, _scissor, Extra1, Extra2, _trueTransform);

        var iCount = OverridePrimCount > 0 ? OverridePrimCount * 3 : Indices.Length;
        
        SpriteRender.Draw(instance, Indices.AsSpan(0, iCount), VertexData.AsSpan());
        
        UiRender.LastRenderCount++;
    }
}