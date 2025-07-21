using MonoClient.UiLib.Enums;
using Common.Vector;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Rendering;
using OpenTK.Mathematics;

namespace MonoClient.UiLib.BuiltIn;

public struct EllipseConfig {
    public int X = 0;
    public int Y = 0;
    public int DiameterX = 0;
    public int DiameterY = 0;
    public int OutlineSize = 0;
    public uint Color = 0x000000;
    public uint OutlineColor = 0x000000;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public bool MouseEnabled = false;

    public EllipseConfig() { }
}

public sealed class Ellipse : Sprite {

    private int _dX;
    private int _dY;
    private int _oSize;

    public Ellipse(EllipseConfig config) {
        X = config.X;
        Y = config.Y;
        _dX = config.DiameterX;
        _dY = config.DiameterY;
        _oSize = config.OutlineSize;
        SetColor(config.Color);
        SetColorSecondary(config.OutlineColor);
        Alpha = config.Alpha;
        SetBaseDimensions(_dX, _dY);
        SetAnchor(config.Anchor);
        SetHitboxType(HitboxType.Ellipse);
        MouseEnabled = config.MouseEnabled;
        TextureId = TextureType.Ellipse;
        
        ResizeBackBuffer();
        FillData();
    }
    
    protected override void ResizeBackBuffer() {
        VertexData = new VertexUi[9];
        Indices = new short[] { 4, 3, 0, 0, 1, 4, 4, 1, 2, 2, 5, 4, 6, 3, 4, 4, 7, 6, 7, 4, 8, 8, 4, 5 };
        base.ResizeBackBuffer();
    }
    
    private void FillData() {
        var (rx, ry) = (_dX / 2, _dY / 2);
        Radii = new IntVector2(rx, ry);
        Extra1.X = rx + _oSize;
        Extra1.Y = ry + _oSize;
        Extra1.Z = _oSize;

        var u = rx + _oSize;
        var v = ry + _oSize;

        VertexData[0] = new VertexUi(new Vector2(0 - _oSize, 0 - _oSize), new Vector2(u, v)); // Top Left
        VertexData[1] = new VertexUi(new Vector2(rx, 0 - _oSize), new Vector2(0f, v)); // Top Center
        VertexData[2] = new VertexUi(new Vector2(rx * 2 + _oSize, 0 - _oSize), new Vector2(u, v)); // Top Right
        
        VertexData[3] = new VertexUi(new Vector2(0 - _oSize, ry), new Vector2(u, 0f)); // Middle Left
        VertexData[4] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        VertexData[5] = new VertexUi(new Vector2(rx * 2 + _oSize, ry), new Vector2(u, 0f)); // Middle Right
        
        VertexData[6] = new VertexUi(new Vector2(0 - _oSize, ry * 2 + _oSize), new Vector2(u, v)); // Bottom Left
        VertexData[7] = new VertexUi(new Vector2(rx, ry * 2 + _oSize), new Vector2(0f, v)); // Bottom Center
        VertexData[8] = new VertexUi(new Vector2(rx * 2 + _oSize, ry * 2 + _oSize), new Vector2(u, v)); // Bottom Right
    }

    public void Resize(int width, int height, int outlineThickness = -1) {
        _dX = width;
        _dY = height;
        SetBaseDimensions(width, height);
        if (outlineThickness > -1)
            _oSize = outlineThickness;
        FillData();
    }
}