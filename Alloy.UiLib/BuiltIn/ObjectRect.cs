using Alloy.UiLib.Core;
using Alloy.UiLib.Data;
using Alloy.UiLib.Rendering;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct ObjectRectConfig {
    public TextureInfo Texture;
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public float Alpha = 1.0f;
    public uint OutlineColor = 0x0;
    public bool OutlineEnabled = true;
    public bool GlowEnabled = true;
    
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public bool MouseEnabled = false;

    public ObjectRectConfig() { }
}

public class ObjectRect : Sprite {
    private AtlasPosition _texture;
    
    private int _width;
    private int _height;
    private bool _outline;
    private bool _glow;

    public ObjectRect(ObjectRectConfig config) {
        X = config.X;
        Y = config.Y;
        _width = config.Width;
        _height = config.Height;
        Alpha = config.Alpha;
        SetColorSecondary(config.OutlineColor);
        Anchor = config.Anchor;
        MouseEnabled = config.MouseEnabled;

        _texture = config.Texture.AtlasPosition;
        TextureId = config.Texture.TextureType;
        _outline = config.OutlineEnabled;
        _glow = config.GlowEnabled;
        
        EnsureBufferCapacity(6);
        FillData();
    }

    private void FillData() {
        VertexData[0] = new VertexUi(new Vector2(0, 0), new Vector2(_texture.U, _texture.V));
        VertexData[1] = new VertexUi(new Vector2(_width, 0), new Vector2(_texture.U + _texture.W, _texture.V));
        VertexData[2] = new VertexUi(new Vector2(_width, _height), new Vector2(_texture.U + _texture.W, _texture.V + _texture.H));
        VertexData[3] = new VertexUi(new Vector2(0, 0), new Vector2(_texture.U, _texture.V));
        VertexData[4] = new VertexUi(new Vector2(_width, _height), new Vector2(_texture.U + _texture.W, _texture.V + _texture.H));
        VertexData[5] = new VertexUi(new Vector2(0, _height), new Vector2(_texture.U, _texture.V + _texture.H));
        
        SetGraphicsBuffer();
        
        Extra1 = new Vector4(_texture.V + _texture.H * 0.4f, _texture.V, _texture.H, _outline ? 1f : -1f);
        Extra2 = new Vector4(_width, _height, _glow ? 1f : -1f, 0);
    }
    
    public void ChangeTexture(TextureInfo info) {
        _texture = info.AtlasPosition;
        TextureId = info.TextureType;
        FillData();
    }
    
    public void Resize(int width, int height) {
        _width = width;
        _height = height;
        FillData();
    }
}