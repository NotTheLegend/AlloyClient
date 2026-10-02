using Alloy.Common;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct ColorRectConfig {
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public Color Color = Color.Black;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.Default;
    public bool MouseEnabled = true;

    public ColorRectConfig() { }
}

public class ColorRect : Sprite {
    
    public ColorRect(ColorRectConfig config) {
        X = config.X;
        Y = config.Y;
        Alpha = config.Alpha;
        Anchor = config.Anchor;
        MouseEnabled = config.MouseEnabled;
        TextureId = TextureType.SolidColor;
        
        Build(config.Width, config.Height);
        SetColor(config.Color);
    }

    private void Build(int w, int h) {
        var data = EnsureBufferCapacity(6);
        
        data[0].Position = new Vector2(0, 0);
        data[1].Position = new Vector2(w, 0);
        data[2].Position = new Vector2(w, h);
        data[3].Position = new Vector2(0, 0);
        data[4].Position = new Vector2(w, h);
        data[5].Position = new Vector2(0, h);
        
        SetGraphicsBuffer();
    }

    public void Resize(int width, int height) => Build(width, height);

    public void SetColor(Color color) => SetColorChannel1(color);
}