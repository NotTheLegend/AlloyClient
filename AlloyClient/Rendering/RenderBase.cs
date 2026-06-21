using System;
using System.Collections.Generic;
using AlloyClient.Assets;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.VertexData;
using Alloy.Common;
using Alloy.Common.Structs;
using OpenTK.Mathematics;


namespace AlloyClient.Rendering;

public abstract class RenderBase : IComparable<RenderBase> {
    public abstract ModelType ModelType { get; }
    
    public abstract bool HasShadow { get; }

    public bool Visible;

    public float RotationAngle;

    public float Size;

    protected internal Entity Entity;
    
    public Vector3 Position = Vector3.Zero;
    public Vector4 UV = Vector4.Zero;
    public Vector4 Scale = Vector4.Zero;
    public Vector4 Rotation = Vector4.Zero;
    public ExtraData Extra;
    public Color Color = Color.Transparent;
    
    public abstract void SetPosition(float x, float y, float z = 0);

    public void SetTexture(AtlasData texture) => SetTexture(texture, false);

    public virtual void SetTexture(AtlasData texture, bool attackFrame) {
        UV = texture.ToVector4();

        var frameMult = attackFrame ? 2f : 1f;
        var w = texture.RawW() - AtlasConfig.Padding * 2;
        var h = texture.RawH() - AtlasConfig.Padding * 2;
        
        // this should be padding * 2 but the attack frame doesnt line up unless its 3 for some fucking reason
        var padW = 1.0f + AtlasConfig.Padding * 3 / texture.RawW();
        var padH = 1.0f + AtlasConfig.Padding * 3 / texture.RawH();
        
        var ratio = w / h / frameMult * MathF.Max(w / frameMult / 8, h / 8);

        var widthScale = 0.75f * ratio * frameMult * padW;
        var heightScale = 0.75f * ratio * padH;
        
        var padX = attackFrame ? widthScale * (0.5f - (AtlasConfig.Padding + w / 4) / texture.RawW()) : 0f;
        var padY = heightScale * (0.5f - AtlasConfig.Padding / texture.RawH());

        Scale = new Vector4(widthScale, heightScale, padX, -padY);
    }

    public abstract void SetVisibility(bool visible);

    public abstract void SetDepth(float depth);
    public abstract void SetName(string name);
    public abstract void SetAlpha(float alpha);
    
    public void SetRotation(float rotation) => RotationAngle = rotation;

    public void SetSize(float size) => Size = size;

    public abstract void Draw(List<VertexObject> targets, double time);
    
    public virtual void DrawShadow() { }

    public int CompareTo(RenderBase other) {
        if (Extra.SortId < other.Extra.SortId) {
            return 1;
        }

        if (Extra.SortId > other.Extra.SortId) {
            return -1;
        }
        
        return 0;
    }
}

public abstract class SubRenderBase {
    public abstract float Height { get; }

    protected RenderBase Parent;
    
    protected Entity Entity;
    
    public Vector3 Position = Vector3.Zero;
    public Vector4 UV = Vector4.Zero;
    public Vector4 Scale = Vector4.Zero;
    public Vector4 Rotation = Vector4.Zero;
    public ExtraData Extra;
    public Color Color = Color.Transparent;

    public void SetDepth(float depth) => Extra.SortId = depth;

    public void SetAlpha(float alpha) => Extra.Alpha = alpha;
    
    public abstract void Draw(float yOffset, List<VertexObject> targets, double time);
}

public struct ExtraData {

    public Vector4 Data => _internal;
    
    public float SortId {
        get => _internal.Y;
        set => _internal.Y = value;
    }

    public float Alpha {
        get => _internal.W;
        set => _internal.W = value;
    }

    private Vector4 _internal;

    public ExtraData(float type, float shade) {
        _internal = new Vector4(type, 0f, shade, 1f);
    }
    
    public ExtraData(float type, float sort, float shade, float alpha) {
        _internal = new Vector4(type, sort, shade, alpha);
    }

    public static ExtraData NewShadedObject(float sortId, float alpha) => new ExtraData(RenderConfig.TypeGameObject, sortId, RenderConfig.Shade, alpha);
}