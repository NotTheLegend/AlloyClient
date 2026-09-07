using Alloy.UiLib.Core;
using Alloy.UiLib.Rendering;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct ColorRectConfig {
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public uint Color = 0x000000;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public bool MouseEnabled = false;

    public ColorRectConfig() { }
}

public sealed class ColorRect : Sprite {
    
    private int _w;
    private int _h;

    public ColorRect(ColorRectConfig config) {
        X = config.X;
        Y = config.Y;
        _w = config.Width;
        _h = config.Height;
        SetColor(config.Color);
        Alpha = config.Alpha;
        Anchor = config.Anchor;
        MouseEnabled = config.MouseEnabled;
        
        TextureId = TextureType.Color;
        
        EnsureBufferCapacity(6);
        FillData();
    }

    private void FillData() {
        VertexData[0] = new VertexUi(new Vector2(0, 0));
        VertexData[1] = new VertexUi(new Vector2(_w, 0));
        VertexData[2] = new VertexUi(new Vector2(_w, _h));
        VertexData[3] = new VertexUi(new Vector2(0, 0));
        VertexData[4] = new VertexUi(new Vector2(_w, _h));
        VertexData[5] = new VertexUi(new Vector2(0, _h));
        
        SetGraphicsBuffer();
    }

    public void Resize(int width, int height) {
        _w = width;
        _h = height;
        FillData();
    }
}