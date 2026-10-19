using Alloy.Common;
using Alloy.UiLib.Core;
using Alloy.UiLib.Data;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct ObjectRectConfig {
    public TextureInfo Texture;
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public float Alpha = 1.0f;
    public Color OutlineColor = Color.Transparent;
    public bool OutlineEnabled = true;
    public bool GlowEnabled = true;
    
    public UiAnchor Anchor = UiAnchor.None;

    public bool MouseEnabled = false;

    public ObjectRectConfig() { }
}

public class ObjectRect : Sprite {
    
    private bool _outline;
    private bool _glow;

    public ObjectRect(ObjectRectConfig config) {
        X = config.X;
        Y = config.Y;
        Alpha = config.Alpha;
        Anchor = config.Anchor;
        MouseEnabled = config.MouseEnabled;
        _outline = config.OutlineEnabled;
        _glow = config.GlowEnabled;
        
        BuildQuad(config.Width, config.Height);
        SetTexture(config.Texture);
        SetOutline(config.OutlineColor);
    }

    private void BuildQuad(int width, int height) {
        var data = GetGraphicsBuffer(6);
        data[0].Position = new Vector2(0, 0);
        data[1].Position = new Vector2(width, 0);
        data[2].Position = new Vector2(width, height);
        data[3].Position = new Vector2(0, 0);
        data[4].Position = new Vector2(width, height);
        data[5].Position = new Vector2(0, height);
        
        SetGraphicsBuffer();
        
        Extra2 = new Vector4(width, height, _glow ? 1f : -1f, 0);
    }

    public void SetTexture(TextureInfo info) {
        TextureId = info.TextureType;
        
        var data = GetGraphicsBuffer(6);
        data[0].Uv = new Vector2(info.AtlasPosition.U, info.AtlasPosition.V);
        data[1].Uv = new Vector2(info.AtlasPosition.U + info.AtlasPosition.W, info.AtlasPosition.V);
        data[2].Uv = new Vector2(info.AtlasPosition.U + info.AtlasPosition.W, info.AtlasPosition.V + info.AtlasPosition.H);
        data[3].Uv = new Vector2(info.AtlasPosition.U, info.AtlasPosition.V);
        data[4].Uv = new Vector2(info.AtlasPosition.U + info.AtlasPosition.W, info.AtlasPosition.V + info.AtlasPosition.H);
        data[5].Uv = new Vector2(info.AtlasPosition.U, info.AtlasPosition.V + info.AtlasPosition.H);
        
        Extra1 = new Vector4(info.AtlasPosition.V + info.AtlasPosition.H * 0.4f, info.AtlasPosition.V, info.AtlasPosition.H, _outline ? 1f : -1f);
    }

    public void SetOutline(Color color) => SetColorChannel2(color);

    public void Resize(int width, int height) => BuildQuad(width, height);
}