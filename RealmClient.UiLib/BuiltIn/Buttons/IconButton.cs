using System;
using OpenTK.Mathematics;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Data;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Rendering;

namespace RealmClient.UiLib.BuiltIn.Buttons;

public struct IconButtonConfig {

    public TextureInfo Texture;
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;
    public Action OnClick = null;
    
    public IconButtonConfig() { }
}

public sealed class IconButton : Sprite {
    private AtlasPosition _texture;

    private uint _activeColor;
    private uint _hoverColor;

    private int _width;
    private int _height;
    
    private bool _leftDown;

    private readonly Action _onClick;

    public IconButton(IconButtonConfig config) {
        _texture = config.Texture.AtlasPosition;
        TextureId = config.Texture.TextureType;
        X = config.X;
        Y = config.Y;
        _width = config.Width;
        _height = config.Height;
        Alpha = config.Alpha;
        SetAnchor(config.Anchor);
        _onClick = config.OnClick;

        MouseEnabled = true;
        AddEventListener(MouseEvent.LeftDown, OnLeftDown);
        AddEventListener(MouseEvent.LeftUp, OnLeftUp);
        
        ResizeBackBuffer();
        FillData();
    }

    private void ResizeBackBuffer() {
        VertexData = new VertexUi[4];
        Indices = [0, 1, 2, 0, 2, 3];
    }

    private void FillData() {
        VertexData[0] = new VertexUi(new Vector2(0, _height), new Vector2(_texture.U, _texture.V + _texture.H)); // Bottom Left
        VertexData[1] = new VertexUi(new Vector2(0, 0), new Vector2(_texture.U, _texture.V)); // Top Left
        VertexData[2] = new VertexUi(new Vector2(_width, 0), new Vector2(_texture.U + _texture.W, _texture.V)); // Top Right
        VertexData[3] = new VertexUi(new Vector2(_width, _height), new Vector2(_texture.U + _texture.W, _texture.V + _texture.H)); // Bottom Right

        Extra1 = new Vector4(_texture.V + _texture.H * 0.4f, _texture.V + _texture.H, -1f, -1f);
        
        SetGraphicsBuffer();
    }

    private void OnLeftDown() {
        _leftDown = true;
    }
    
    private void OnLeftUp() {
        if (_leftDown) {
            _onClick?.Invoke();
        }
        _leftDown = false;
    }

    public void ChangeTexture(TextureInfo info) {
        _texture = info.AtlasPosition;
        TextureId = info.TextureType;
        FillData();
    }
}