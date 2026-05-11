using System;
using Alloy.Engine.Common;
using Alloy.Engine.Graphics;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Components;

public static class MinimapTexture {
    
    private static Texture _texture;
    private static Color[] _data;
    
    private static bool _needsUpdate;
    private static int _minX;
    private static int _minY;
    private static int _maxX;
    private static int _maxY;

    public static void Init(out Texture texture) {
        _data = new Color[4096 * 4096];
        _texture = new Texture(new ReadOnlySpan<Color>(_data), 4096, 4096);

        _minX = _minY = 4096;
        _maxX = _maxY = 0;
        texture = _texture;
    }
    
    public static void ClearData() {
        for (var i = 0; i < _data.Length; i++) {
            _data[i] = Color.Black;
        }

        _texture.SetData(_data, 4096, 4096);
    }
    
    public static void UncoverTile(int x, int y, Color color) {
        _minX = Math.Min(x, _minX);
        _minY = Math.Min(y, _minY);
        _maxX = Math.Max(x, _maxX);
        _maxY = Math.Max(y, _maxY);

        _data[4096 * y + x] = color;
        _needsUpdate = true;
    }
    
    public static void PreDrawUpdate() {
        if (!_needsUpdate) return;
        
        var (w, h) = (_maxX + 1 - _minX, _maxY + 1 - _minY);
        if (w <= 0 || h <= 0) return;

        var newData = new Color[w * h];

        var idx = 0;

        for (var y = _minY; y < _maxY + 1; y++) {
            Array.Copy(_data, 4096 * y + _minX, newData, idx * w, w);
            idx++;
        }

        _texture.SetData(new ReadOnlySpan<Color>(newData), new Vector4i(_minX, _minY, w, h));

        _needsUpdate = false;
        _minX = _minY = 4096;
        _maxX = _maxY = 0;
    }
    
}