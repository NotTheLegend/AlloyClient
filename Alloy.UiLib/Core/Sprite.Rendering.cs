using System;
using Alloy.UiLib.Rendering;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

public partial class Sprite {

    private bool _noRenderData = true;

    /// <summary>
    /// Internal workings assumes vertex data has (0,0) in top left and (w,h) in bottom right,
    /// If you don't start at (0,0) width and height calculations will be off
    /// </summary>
    /// <exception cref="Exception">throws if data is incomplete to form 1 primitive</exception>
    protected void SetGraphicsBuffer() {
        if (Indices?.Length is > 0 and < 3) {
            throw new Exception("Primitives require at least 3 indices");
        }

        if (Indices is not null && Indices.Length % 3 != 0) {
            throw new Exception("Total indices not divisible by 3");
        }

        if (VertexData?.Length is > 0 and < 3) {
            throw new Exception("Primitives require at least 3 vertices");
        }

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
        if (!Visible) {
            return;
        }

        if (!_noRenderData && _trueAlpha != 0f && OverridePrimCount != 0) {
            DrawInternal();
        }

        var span = GetChildrenSpan();
        foreach (var child in span) {
            child.Draw();
        }
    }

    private void DrawInternal() {
        var vertexMatrix = new SpriteVertexMatrix(
            new Vector4(_worldTransform.M11, _worldTransform.M12, _worldTransform.TX, 0f),
            new Vector4(_worldTransform.M21, _worldTransform.M22, _worldTransform.TY, 0f)
        );

        var instance = new SpriteInstanceData(vertexMatrix, Color, ColorSecondary, _info, _scissor, Extra1, Extra2, _trueTransform);

        var iCount = OverridePrimCount > 0 ? OverridePrimCount * 3 : Indices.Length;

        SpriteRender.Draw(instance, Indices.AsSpan(0, iCount), VertexData.AsSpan());

        UiRender.LastRenderCount++;
    }
}
