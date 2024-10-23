using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.State;
using MonoClient.UiLib;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud;

public sealed class Minimap : Sprite {
    
    private const int MapSize = 246;
    
    private static Minimap _instance;
    public static Minimap Instance => _instance ??= new Minimap();

    private bool _needsUpdate;
    private int _minX;
    private int _minY;
    private int _maxX;
    private int _maxY;

    private Texture2D _texture;
    private Color[] _data;
    
    private float _zoom = 4.0f;
    private float _maxZoom;
    private float _zoomStep;
    private float _size;
    
    //todo add static objects color

    private Minimap() {
        SetBaseDimensions(MapSize, MapSize);
        TextureId = TextureType.Minimap;

        ResizeBackBuffer();
        FillData();
    }

    protected override void ResizeBackBuffer() {
        VertexData = new VertexUi[4];
        Indices = new short[] { 0, 1, 2, 0, 2, 3 };
        base.ResizeBackBuffer();
    }

    private void FillData() {
        VertexData[0] = new VertexUi(new Vector2(0, 0)); //Top Left
        VertexData[1] = new VertexUi(new Vector2(MapSize, 0)); //Top Right
        VertexData[2] = new VertexUi(new Vector2(MapSize, MapSize)); //Bottom Right
        VertexData[3] = new VertexUi(new Vector2(0, MapSize)); //Bottom Left
    }
    
    public void ZoomHandle(int zoom) {
        _zoom += _zoomStep * zoom;
        _zoom = Math.Max(1, Math.Min(_maxZoom, _zoom));
    }

    public static Texture2D Init(GraphicsDevice graphics) {
        var mm = Instance;
        mm._texture = new Texture2D(graphics, 4096, 4096);
        mm._data = new Color[4096 * 4096];

        mm._minX = mm._minY = 4096;
        mm._maxX = mm._maxY = 0;
        return mm._texture;
    }
    
    public void OnMapEnter(int w, int h) {
        var size = (float)Math.Max(w, h);
        _maxZoom = size / 32;
        _zoomStep = size / Settings.DefaultScreenWidth ;
        _size = size;

        for (int i = 0; i < _data.Length; i++) {
            _data[i] = Color.Black;
        }

        _texture.SetData(_data, 0, 4096 * 4096);
    }

    public void UncoverTile(int x, int y, Color color) {
        _minX = Math.Min(x, _minX);
        _minY = Math.Min(y, _minY);
        _maxX = Math.Max(x, _maxX);
        _maxY = Math.Max(y, _maxY);

        _data[4096 * y + x] = color;
        _needsUpdate = true;
    }

    protected override void OnUpdate(GameTime time) {
        if (Map.LocalPlayer == null) return;

        var pos = Map.LocalPlayer.Position;
        var size = _size / _zoom / 2.0f;

        var x1 = pos.X - size;
        var x2 = pos.X + size;
        var y1 = pos.Y - size;
        var y2 = pos.Y + size;
        VertexData[0].UV = new Vector2(x1 / 4096, y1 / 4096);
        VertexData[1].UV = new Vector2(x2 / 4096, y1 / 4096);
        VertexData[2].UV = new Vector2(x2 / 4096, y2 / 4096);
        VertexData[3].UV = new Vector2(x1 / 4096, y2 / 4096);
    }

    public void PreDrawUpdate() {
        if (!_needsUpdate) return;
        
        (var w, var h) = (_maxX + 1 - _minX, _maxY + 1 - _minY);
        if (w <= 0 || h <= 0) return;

        var newData = new Color[w * h];

        var idx = 0;

        for (var y = _minY; y < _maxY + 1; y++) {
            Array.Copy(_data, 4096 * y + _minX, newData, idx * w, w);
            idx++;
        }

        _texture.SetData(0, new Rectangle(_minX, _minY, w, h), newData, 0, newData.Length);

        _needsUpdate = false;
        _minX = _minY = 4096;
        _maxX = _maxY = 0;
    }
    
}