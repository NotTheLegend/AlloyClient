using System;
using Alloy.Common;
using Alloy.UiLib.Rendering;
using Alloy.UiLib.Utils;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

public abstract class Sprite : DisplayContainer {

    private int _stateIndex;
    private bool _noRenderData = true;
    private Vector2i _selfContentDimension = Vector2i.Zero;
    private Vector2i _selfContentOffset = Vector2i.Zero;

    protected TextureType TextureId;
    protected VertexUi[] VertexData;

    private protected sealed override Vector2i GetSelfContentDimensions() => _selfContentDimension;

    internal sealed override void SetStageReference(Stage stage) {
        if (stage is not null) {
            _stateIndex = NewRender.StatePool.Pop();
            SetGraphicsBuffer();
        } else {
            NewRender.StatePool.Push(_stateIndex);
        }
        
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
                //vertex.StateIndex = _stateIndex;
            }

            _selfContentDimension = new Vector2i((int)(x1 - x), (int)(y1 - y));
            _selfContentOffset = new Vector2i((int)x, (int)y);
        } else {
            _selfContentDimension = Vector2i.Zero;
            _selfContentOffset = Vector2i.Zero;
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
        
        var vertexMatrix = new SpriteVertexMatrix(State.Scale, 0f, new Vector2(State.X, State.Y), new Vector2(0, 0));
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
    //protected ushort[] Indices;
    protected Vector4 Extra1;
    protected Vector4 Extra2;
    protected Vector2 Radii;
    protected int OverridePrimCount;
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
    
    
    // to impl
    public void SetHitboxType(CollisionType type) {}
    protected virtual bool CustomHitbox(Vector2i pos) {
        throw new MissingMethodException("Sprite must define override for CustomHitbox");
    }

    protected void EnsureBufferCapacity(int length) {
        if (length % 3 != 0) {
            throw new Exception("length needs to be a multiple of 3");
        }
        
        if (VertexData is null) {
            VertexData = new VertexUi[length];
            return;
        }

        if (VertexData.Length > length) {
            OverridePrimCount = length / 3;
            return;
        }
        
        Array.Resize(ref VertexData, length);
    }
}