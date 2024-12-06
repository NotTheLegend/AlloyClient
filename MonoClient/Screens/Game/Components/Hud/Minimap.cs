using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.State;
using MonoClient.UiLib;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Utils.Signals;

namespace MonoClient.Screens.Game.Components.Hud;

public sealed class Minimap : Sprite {

    public static readonly SingleSignal<int> OnZoom = new();
    public static readonly SingleSignal<int, int> OnNewMap = new();
    
    private const int MapSize = 246;
    
    private float _zoom = 4.0f;
    private float _maxZoom;
    private float _zoomStep;
    private float _size;

    public Minimap() {
        SetBaseDimensions(MapSize, MapSize);
        TextureId = TextureType.Minimap;

        ResizeBackBuffer();
        FillData();
        
        OnZoom.Set(ZoomHandle);
        OnNewMap.Set(OnMapEnter);
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
    
    private void ZoomHandle(int zoom) {
        _zoom += _zoomStep * zoom;
        _zoom = Math.Max(1, Math.Min(_maxZoom, _zoom));
    }
    
    private void OnMapEnter(int w, int h) {
        var size = (float)Math.Max(w, h);
        _maxZoom = size / 32;
        _zoomStep = size / Settings.DefaultScreenWidth ;
        _size = size;

        MinimapData.ClearData();
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
}