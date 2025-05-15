using Microsoft.Xna.Framework;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Data;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Rendering;

namespace MonoClient.UiLib.BuiltIn;

public struct NineSliceConfig {
    public required string SliceData;
    public bool Padding = false;
    public int CutX = 0;
    public int CutY = 0;
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public bool MouseEnabled = false;

    public NineSliceConfig() { }
}

public sealed class NineSliceRect : Sprite {
    private SliceData _slice;
    private bool _padding;
    private int _w;
    private int _h;
    
    private float _cutX;
    private float _cutY;

    public NineSliceRect(NineSliceConfig config) {
        _slice = SliceDataManager.GetSlice(config.SliceData);
        _padding = config.Padding;
        X = config.X;
        Y = config.Y;
        _w = config.Width;
        _h = config.Height;
        Alpha = config.Alpha;
        SetBaseDimensions(_w, _h);
        SetAnchor(config.Anchor);
        _cutX = config.CutX;
        _cutY = config.CutY;
        MouseEnabled = config.MouseEnabled;
        
        TextureId = TextureType.UiSlice;
        
        ResizeBackBuffer();
        Resize(_w, _h);
        
    }
    
    protected override void ResizeBackBuffer() {
        VertexData = new VertexUi[4];
        Indices = new short[] { 0, 1, 2, 0, 2, 3 };
        var data = _slice.AtlasData;
        if (!_padding)
            data.RemovePadding();
        Extra1 = new Vector4(data.U, data.U + data.W, data.V, data.V + data.H);
        base.ResizeBackBuffer();
    }

    public void Resize(int w, int h) {
        SetBaseDimensions(_w = w, _h = h);

        VertexData[0] = new VertexUi(new Vector2(0, 0), new Vector2(0, 0));
        VertexData[1] = new VertexUi(new Vector2(_w, 0), new Vector2(1, 0));
        VertexData[2] = new VertexUi(new Vector2(_w, _h), new Vector2(1, 1));
        VertexData[3] = new VertexUi(new Vector2(0, _h), new Vector2(0, 1));

        var scaleX = _w / (_cutX * 2f);
        if (scaleX > 1.0f)
            scaleX = 1.0f;

        var scaleY = _h / (_cutY * 2f);
        if (scaleY > 1.0f)
            scaleY = 1.0f;

        Extra2 = new Vector4(_slice.Cuts.X * scaleX, _slice.Cuts.Y * scaleY, _cutX / _w * scaleX, _cutY / _h * scaleY);
    }
}