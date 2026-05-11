using System;
using AlloyClient.State;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.BuiltIn.Buttons;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;
using AlloyClient.UiLib.Extra;
using AlloyClient.UiLib.Rendering;
using AlloyClient.UiLib.Signals;
using AlloyClient.Utils;
using Alloy.Common;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Components.Hud;

public sealed class Minimap : Sprite {

    public static readonly SingleSignal<int> OnZoom = new();
    public static readonly SingleSignal<int, int> OnNewMap = new();

    private static readonly ColorTransform DefaultCt = new (1f, 1f, 1f, 1f);
    private static readonly ColorTransform FadeCt = new (0.5f, 0.5f, 0.5f, 1f);
    
    public const int MapSize = 230;
    
    private float _zoom = 4.0f;
    private float _maxZoom;
    private float _zoomStep;
    private float _size;

    private bool _mouseOver;

    private readonly MinimapLayer _layer;

    private readonly IconButton _zoomIn;
    private readonly IconButton _zoomOut;
    private readonly ObjectRect _arrow;

    public Minimap() {
        TextureId = TextureType.Minimap;

        ResizeBackBuffer();
        FillData();
        
        OnZoom.Set(ZoomHandle);
        OnNewMap.Set(OnMapEnter);

        _layer = new MinimapLayer();
        AddChild(_layer);

        _zoomIn = new IconButton(new IconButtonConfig {
            Texture = TextureHelper.FromGameAtlas("lofiInterface", 54, false),
            X = MapSize,
            Y = 0,
            Width = 24,
            Height = 24,
            Anchor = UiAnchor.RightTop,
            OnClick = () => ZoomHandle(1)
        });
        AddChild(_zoomIn);
        
        _zoomOut = new IconButton(new IconButtonConfig {
            Texture = TextureHelper.FromGameAtlas("lofiInterface", 55, false),
            X = MapSize,
            Y = _zoomIn.Height + 4,
            Width = 24,
            Height = 24,
            Anchor = UiAnchor.RightTop,
            OnClick = () => ZoomHandle(-1)
        });
        AddChild(_zoomOut);

        _arrow = new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.FromGameAtlas("lofiInterface", 54, false),
            X = MapSize / 2,
            Y = MapSize / 2,
            Width = 9,
            Height = 36,
            Anchor = UiAnchor.Middle
        });
        _arrow.ColorTransformation = new ColorTransform(0f, 0f, 1f, 1f);
        AddChild(_arrow);
        
        AddEventListener(Event.EnterFrame, OnFrameEnter);
        
        // :smallbrain: not valid callbacks, doesnt support lambdas /shrug >:
        //AddEventListener(MouseEventId.MouseOver, () => _mouseOver = true);
        //AddEventListener(MouseEventId.MouseOut, () => _mouseOver = false);
    }

    private void UpdateButtons() {
        if (_zoom <= 1f) {
            _zoomIn.ColorTransformation = Transforms.Default;
            _zoomOut.ColorTransformation = Transforms.Dark;
        } else if (_zoom >= _maxZoom) {
            _zoomIn.ColorTransformation = Transforms.Dark;
            _zoomOut.ColorTransformation = Transforms.Default;
        } else {
            _zoomIn.ColorTransformation = Transforms.Default;
            _zoomOut.ColorTransformation = Transforms.Default;
        }
    }

    private void ResizeBackBuffer() {
        VertexData = new VertexUi[4];
        Indices = [0, 1, 2, 0, 2, 3];
    }

    private void FillData() {
        VertexData[0] = new VertexUi(new Vector2(0, 0)); //Top Left
        VertexData[1] = new VertexUi(new Vector2(MapSize, 0)); //Top Right
        VertexData[2] = new VertexUi(new Vector2(MapSize, MapSize)); //Bottom Right
        VertexData[3] = new VertexUi(new Vector2(0, MapSize)); //Bottom Left
        
        SetGraphicsBuffer();
    }
    
    private void ZoomHandle(int zoom) {
        _zoom += _zoomStep * zoom;
        _zoom = Math.Max(1, Math.Min(_maxZoom, _zoom));
        UpdateButtons();
    }
    
    private void OnMapEnter(int w, int h) {
        var size = (float)Math.Max(w, h);
        _maxZoom = size / 32;
        _zoomStep = size / Settings.DefaultScreenWidth ;
        _size = size;
        MinimapTexture.ClearData();
    }
    
    private void OnFrameEnter() {
        if (Map.LocalPlayer == null) return;

        _arrow.Rotation = -Camera.CameraAngle;

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
        
        _layer.SetSize(size);
    }
}