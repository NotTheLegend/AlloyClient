using MonoClient.UiLib.Core;
using MonoClient.UiLib.Data;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Rendering;
using OpenTK.Mathematics;

namespace MonoClient.UiLib.BuiltIn;

public struct ObjectRectConfig {
    public TextureInfo Texture;
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public float Alpha = 1.0f;
    public uint OutlineColor = 0x0;
    
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public bool MouseEnabled = false;

    public ObjectRectConfig() { }
}

public class ObjectRect : Sprite {
    private AtlasPosition _texture;
    
    private int _width;
    private int _height;

    public ObjectRect(ObjectRectConfig config) {
        X = config.X;
        Y = config.Y;
        _width = config.Width;
        _height = config.Height;
        Alpha = config.Alpha;
        SetColorSecondary(config.OutlineColor);
        SetBaseDimensions(_width, _height);
        SetAnchor(config.Anchor);
        MouseEnabled = config.MouseEnabled;

        _texture = config.Texture.AtlasPosition;
        TextureId = config.Texture.TextureType;

        ResizeBackBuffer();
        FillData();
    }
    
    protected override void ResizeBackBuffer() {
        VertexData = new VertexUi[4];
        Indices = new short[] { 0, 1, 2, 0, 2, 3 };
        base.ResizeBackBuffer();
    }

    private void FillData() {
        VertexData[0] = new VertexUi(new Vector2(0, _height), new Vector2(_texture.U, _texture.V + _texture.H));
        VertexData[1] = new VertexUi(new Vector2(0, 0), new Vector2(_texture.U, _texture.V));
        VertexData[2] = new VertexUi(new Vector2(_width, 0), new Vector2(_texture.U + _texture.W, _texture.V));
        VertexData[3] = new VertexUi(new Vector2(_width, _height), new Vector2(_texture.U + _texture.W, _texture.V + _texture.H));
        
        Extra1 = new Vector4(_texture.V + _texture.H * 0.4f, _texture.V + _texture.H, -1f, -1f);
    }
    
    public void ChangeTexture(TextureInfo info) {
        _texture = info.AtlasPosition;
        TextureId = info.TextureType;
        FillData();
    }
    
    public void Resize(int width, int height) {
        _width = width;
        _height = height;
        SetBaseDimensions(width, height);
        FillData();
    }
}