using Alloy.Common;
using Alloy.UiLib.Core;
using Alloy.UiLib.Rendering;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct EllipseConfig {
    public int X = 0;
    public int Y = 0;
    public int DiameterX = 0;
    public int DiameterY = 0;
    public int OutlineSize = 0;
    public Color Color = Color.Black;
    public Color OutlineColor = Color.Black;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.Default;

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
        Alpha = config.Alpha;
        Anchor = config.Anchor;
        HitboxType = CollisionType.Ellipse;
        MouseEnabled = config.MouseEnabled;
        TextureId = TextureType.Ellipse;
        
        FillData();
        SetChannel1Color(config.Color);
        SetChannel2Color(config.OutlineColor);
    }
    
    private void FillData() {
        var (rx, ry) = (_dX / 2, _dY / 2);
        Radii = new Vector2i(rx, ry);
        Extra1.X = rx + _oSize;
        Extra1.Y = ry + _oSize;
        Extra1.Z = _oSize;

        var u = rx + _oSize;
        var v = ry + _oSize;
        
        var data = GetGraphicsBuffer(24);
        
        data[0] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        data[1] = new VertexUi(new Vector2(0 - _oSize, ry), new Vector2(u, 0f)); // Middle Left
        data[2] = new VertexUi(new Vector2(0 - _oSize, 0 - _oSize), new Vector2(u, v)); // Top Left
        
        data[3] = new VertexUi(new Vector2(0 - _oSize, 0 - _oSize), new Vector2(u, v)); // Top Left
        data[4] = new VertexUi(new Vector2(rx, 0 - _oSize), new Vector2(0f, v)); // Top Center
        data[5] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        
        data[6] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        data[7] = new VertexUi(new Vector2(rx, 0 - _oSize), new Vector2(0f, v)); // Top Center
        data[8] = new VertexUi(new Vector2(rx * 2 + _oSize, 0 - _oSize), new Vector2(u, v)); // Top Right
        
        data[9] = new VertexUi(new Vector2(rx * 2 + _oSize, 0 - _oSize), new Vector2(u, v)); // Top Right
        data[10] = new VertexUi(new Vector2(rx * 2 + _oSize, ry), new Vector2(u, 0f)); // Middle Right
        data[11] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        
        data[12] = new VertexUi(new Vector2(0 - _oSize, ry * 2 + _oSize), new Vector2(u, v)); // Bottom Left
        data[13] = new VertexUi(new Vector2(0 - _oSize, ry), new Vector2(u, 0f)); // Middle Left
        data[14] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        
        data[15] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        data[16] = new VertexUi(new Vector2(rx, ry * 2 + _oSize), new Vector2(0f, v)); // Bottom Center
        data[17] = new VertexUi(new Vector2(0 - _oSize, ry * 2 + _oSize), new Vector2(u, v)); // Bottom Left
        
        data[18] = new VertexUi(new Vector2(rx, ry * 2 + _oSize), new Vector2(0f, v)); // Bottom Center
        data[19] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        data[20] = new VertexUi(new Vector2(rx * 2 + _oSize, ry * 2 + _oSize), new Vector2(u, v)); // Bottom Right
        
        data[21] = new VertexUi(new Vector2(rx * 2 + _oSize, ry * 2 + _oSize), new Vector2(u, v)); // Bottom Right
        data[22] = new VertexUi(new Vector2(rx, ry), new Vector2(0f, 0f)); // Center
        data[23] = new VertexUi(new Vector2(rx * 2 + _oSize, ry), new Vector2(u, 0f)); // Middle Right
        
        SetGraphicsBuffer();
    }

    public void Resize(int width, int height, int outlineThickness = -1) {
        _dX = width;
        _dY = height;
        if (outlineThickness > -1)
            _oSize = outlineThickness;
        FillData();
    }
}