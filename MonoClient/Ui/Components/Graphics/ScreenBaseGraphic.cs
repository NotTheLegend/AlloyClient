using Microsoft.Xna.Framework;
using MonoClient.UiLib;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Ui.Components.Graphics;

public struct ScreenGraphicConfig {

    public bool TitleScreen = false;
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public UiAnchor Anchor = UiAnchor.LeftTop;
    
    public ScreenGraphicConfig() { }
}

public sealed class ScreenGraphic : Sprite {
    
    private readonly int _width;
    private readonly int _height;

    public ScreenGraphic(ScreenGraphicConfig config) {
        X = config.X;
        Y = config.Y;
        _width = config.Width;
        _height = config.Height;
        SetAnchor(config.Anchor);

        TextureId = config.TitleScreen ? TextureType.TitleGraphic : TextureType.TitleBackground;

        SetBaseDimensions(_width, _height);
        ResizeBackBuffer();
        FillData();
    }
    
    protected override void ResizeBackBuffer() {
        VertexData = new VertexUi[4];
        Indices = new short[] { 0, 1, 2, 0, 2, 3 };
        base.ResizeBackBuffer();
    }

   private void FillData() {
       VertexData[0] = new VertexUi(new Vector2(0, _height), new Vector2(0f, 1f));
       VertexData[1] = new VertexUi(new Vector2(0, 0), new Vector2(0f, 0f));
       VertexData[2] = new VertexUi(new Vector2(_width, 0), new Vector2(1f, 0f));
       VertexData[3] = new VertexUi(new Vector2(_width, _height), new Vector2(1f, 1f));
    }
    
}