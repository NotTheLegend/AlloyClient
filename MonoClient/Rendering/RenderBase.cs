using System;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Assets;
using MonoClient.Objects;


namespace MonoClient.Rendering;

public abstract class RenderBase : IComparable<RenderBase> {
    public abstract ModelType ModelType { get; }
    
    public abstract bool HasShadow { get; }

    public bool Visible;

    protected internal Entity Entity;
    
    public Vector3 Position = Vector3.Zero;
    public Vector4 UV = Vector4.Zero;
    public Vector4 Scale = Vector4.Zero;
    public Vector4 Rotation = Vector4.Zero;
    public ExtraData Extra;
    public Color Color = Color.Transparent;
    
    public abstract void SetPosition(float x, float y, float z = 0);

    public void SetTexture(AtlasData texture) => SetTexture(texture, false);
    
    public abstract void SetTexture(AtlasData texture, bool attackFrame);

    public abstract void SetVisibility(bool visible);

    public abstract void SetDepth(float depth);
    public abstract void SetName(string name);
    public abstract void SetAlpha(float alpha);

    public abstract void Draw();
    
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
    
    public abstract void Draw(float yOffset);
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
}