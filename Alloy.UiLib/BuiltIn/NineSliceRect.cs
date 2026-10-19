using Alloy.Common;
using Alloy.UiLib.Core;
using Alloy.UiLib.Data;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct NineSliceConfig {
    public required string SliceData;
    public int CutX = 0;
    public int CutY = 0;
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.Default;

    public bool MouseEnabled = false;

    public NineSliceConfig() { }
}

public sealed class NineSliceRect : Sprite {
    private SliceData _slice;
    private int _w;
    private int _h;
    
    private float _cutX;
    private float _cutY;

    public NineSliceRect(NineSliceConfig config) {
        _slice = SliceLookup.GetSlice(config.SliceData);
        X = config.X;
        Y = config.Y;
        _w = config.Width;
        _h = config.Height;
        Alpha = config.Alpha;
        Anchor = config.Anchor;
        _cutX = config.CutX;
        _cutY = config.CutY;
        MouseEnabled = config.MouseEnabled;
        
        TextureId = TextureType.UiSlice;
        
        ResizeBackBuffer();
        Resize(_w, _h);
    }
    
    private void ResizeBackBuffer() {
        
        var data = _slice.AtlasData;
        Extra1 = new Vector4(data.U, data.U + data.W, data.V, data.V + data.H);
    }

    public void Resize(int w, int h) {
        _w = w; 
        _h = h;
        
        var data = GetGraphicsBuffer(6);

        data[0].PositionUv = (new Vector2(0, 0), new Vector2(0, 0));
        data[1].PositionUv = (new Vector2(_w, 0), new Vector2(1, 0));
        data[2].PositionUv = (new Vector2(_w, _h), new Vector2(1, 1));
        data[3].PositionUv = (new Vector2(0, 0), new Vector2(0, 0));
        data[4].PositionUv = (new Vector2(_w, _h), new Vector2(1, 1));
        data[5].PositionUv = (new Vector2(0, _h), new Vector2(0, 1));
        
        SetGraphicsBuffer();

        var scaleX = _w / (_cutX * 2f);
        if (scaleX > 1.0f)
            scaleX = 1.0f;

        var scaleY = _h / (_cutY * 2f);
        if (scaleY > 1.0f)
            scaleY = 1.0f;

        Extra2 = new Vector4(_slice.Cuts.X * scaleX, _slice.Cuts.Y * scaleY, _cutX / _w * scaleX, _cutY / _h * scaleY);
    }

    public void SetColor(Color color) => SetColorChannel1(color);
}