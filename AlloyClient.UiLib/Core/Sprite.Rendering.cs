using System;
using Alloy.Engine.Common;
using AlloyClient.UiLib.Rendering;
using OpenTK.Mathematics;

namespace AlloyClient.UiLib.Core;

public partial class Sprite {
    
    private bool _noRenderData = true;
    
    /// <summary>
    /// Internal workings assumes vertex data has (0,0) in top left and (w,h) in bottom right,
    /// If you don't start at (0,0) width and height calculations will be off
    /// </summary>
    /// <exception cref="Exception">throws if data is incomplete to form 1 primitive</exception>
    protected void SetGraphicsBuffer() {
        if (Indices?.Length is > 0 and < 3) throw new Exception("Primitives require at least 3 indices");
        if (Indices is not null && Indices.Length % 3 != 0) throw new Exception("Total indices not divisible by 3");
        if (VertexData?.Length is > 0 and < 3) throw new Exception("Primitives require at least 3 vertices");
        
        _noRenderData = VertexData is null || Indices is null || VertexData.Length < 3 || Indices.Length < 3;

        if (!_noRenderData) {
            var w = 0f;
            var h = 0f;
            foreach (var vertex in VertexData!) {
                w = Math.Max(w, vertex.Position.X);
                h = Math.Max(h, vertex.Position.Y);
            }

            SelfContentWidth = (int)w;
            SelfContentHeight = (int)h;
        } else {
            SelfContentWidth = SelfContentHeight = 0;
        }
        
        UpdateBounds();
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

        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.Draw();
        }
    }

    //private ushort[] ind = [0, 1, 2, 0, 2, 3];
    //private VertexUi[] vert = new VertexUi[4];
    
    private void DrawInternal() {
        var vertexMatrix = new SpriteVertexMatrix(_trueScale, _trueRotation, new Vector2(_trueX, _trueY), new Vector2(_anchorX, _anchorY));
        var instance = new SpriteInstanceData(vertexMatrix, Color, ColorSecondary, _info, _scissor, Extra1, Extra2, _trueTransform);

        var iCount = OverridePrimCount > 0 ? OverridePrimCount * 3 : Indices.Length;

        //vert[0] = new VertexUi(new Vector2(0, 0), new Color(1f, 0f, 0f));
        //vert[1] = new VertexUi(new Vector2(Width, 0), new Color(1f, 0f, 0f));
        //vert[2] = new VertexUi(new Vector2(Width, Height), new Color(1f, 0f, 0f));
        //vert[3] = new VertexUi(new Vector2(0, Height), new Color(1f, 0f, 0f));
        //var inst = new SpriteInstanceData(vertexMatrix, Color, ColorSecondary, new Vector2((float) TextureType.Color, 0.5f), _scissor, Extra1, Extra2, _trueTransform);
        //SpriteRender.Draw(inst, ind.AsSpan(), vert.AsSpan());
        
        SpriteRender.Draw(instance, Indices.AsSpan(0, iCount), VertexData.AsSpan());
        
        UiRender.LastRenderCount++;
    }
}