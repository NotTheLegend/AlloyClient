using System;
using Common;
using Common.Structs;
using MonoClient.Assets;
using MonoClient.Assets.Libraries;
using MonoClient.Assets.XmlStructs;
using MonoClient.Rendering;
using MonoClient.Rendering.VertexData;
using MonoClient.Objects;
using OpenTK.Mathematics;

namespace MonoClient;

public class MapTile(int x, int y) {
    public readonly int X = x;
    public readonly int Y = y;

    public ushort Type = 0xFF;
    public GroundProperties GroundProperties = GroundLibrary.TypeToGroundProps[0xFF];
    public TextureData TextureData = GroundLibrary.TypeToTextureData[0xFF];

    private Entity _occupiedObject;

    public Entity OccupiedObject {
        get => _occupiedObject;
        set => SetMinimapColor(_occupiedObject = value);
    }

    private AtlasData _texture;
    private Color _color;
    private Vector2 _blendUV;

    // Shader data
    private Vector4 _positionOffset;
    private Vector4 _uv;
    private Vector4 _animate;
    private Vector4 _blendTopBottom = new(-1f);
    private Vector4 _blendLeftRight = new(-1f);
    private Vector4 _cornerBottom = new(-1);
    private Vector4 _cornerTop = new(-1);

    private void SetMinimapColor(Entity entity) {
        if (entity != null && entity.Properties.Static && entity.Properties.OccupySquare && !entity.Properties.NoMiniMap) {
            MinimapTexture.UncoverTile(X, Y, entity.GetDominateColor());
        } else {
            MinimapTexture.UncoverTile(X, Y, _color);
        }
    }

    public void DrawTile() {
        Render.DrawTile(new VertexTile(_positionOffset, _uv, _animate, _blendLeftRight, _blendTopBottom, _cornerBottom, _cornerTop));
    }
    
    public void DrawEditorTile() {
        Render.DrawEditorTile(new VertexTile(_positionOffset, _uv, _animate, _blendLeftRight, _blendTopBottom, _cornerBottom, _cornerTop));
    }

    public void SetType(ushort type) {
        Type = type;
        GroundProperties = GroundLibrary.TypeToGroundProps[type];
        TextureData = GroundLibrary.TypeToTextureData[type];

        _texture = TextureData.GetTexture(out _color, true);
        _texture.RemovePadding();
        _uv = _texture.ToVector4();
        _blendUV = new Vector2(_uv.X, _uv.Y);
        
        SetMinimapColor(_occupiedObject);

        var offx = GroundProperties.XOffset;
        var offy = GroundProperties.YOffset;

        if (GroundProperties.RandomOffset) {
            offx = (int)(Random.Shared.NextSingle() * _texture.RawW()) / (float)_texture.RawW();
            offy = (int)(Random.Shared.NextSingle() * _texture.RawH()) / (float)_texture.RawH();
        }
        
        _positionOffset = new Vector4(X, Y, offx, offy);
        
        var animate = GroundProperties.Animate;
        switch (animate.Type) {
            case GroundAnimate.State.Wave:
                _animate.X = animate.DeltaX;
                _animate.Y = animate.DeltaY;
                break;
            case GroundAnimate.State.Flow:
                _animate.Z = animate.DeltaX;
                _animate.W = animate.DeltaY;
                break;
        }

        SetEdgeBlends();
        UpdateCorners();
    }

    public bool IsWalkable() {
        return !GroundProperties.NoWalk && (OccupiedObject == null || !OccupiedObject.Properties.OccupySquare);
    }

    private void SetEdgeBlends() {
        var currPrio = GroundProperties.BlendPriority;
        
        // Top
        if (GetTile(X, Y - 1, out var tile, out var tilePrio)) {
            if (tilePrio > currPrio) {
                _blendTopBottom.X = tile._blendUV.X;
                _blendTopBottom.Y = tile._blendUV.Y;
                tile._blendTopBottom.Z = -1;
                tile._blendTopBottom.W = -1;
            } else if (currPrio > tilePrio) {
                _blendTopBottom.X = -1;
                _blendTopBottom.Y = -1;
                tile._blendTopBottom.Z = _blendUV.X;
                tile._blendTopBottom.W = _blendUV.Y;
            } else {
                _blendTopBottom.X = -1;
                _blendTopBottom.Y = -1;
                tile._blendTopBottom.Z = -1;
                tile._blendTopBottom.W = -1;
            }
        }
        
        // Right
        if (GetTile(X + 1, Y, out tile, out tilePrio)) {
            if (tilePrio > currPrio) {
                _blendLeftRight.Z = tile._blendUV.X;
                _blendLeftRight.W = tile._blendUV.Y;
                tile._blendLeftRight.X = -1;
                tile._blendLeftRight.Y = -1;
            } else if (currPrio > tilePrio) {
                _blendLeftRight.Z = -1;
                _blendLeftRight.W = -1;
                tile._blendLeftRight.X = _blendUV.X;
                tile._blendLeftRight.Y = _blendUV.Y;
            } else {
                _blendLeftRight.Z = -1;
                _blendLeftRight.W = -1;
                tile._blendLeftRight.X = -1;
                tile._blendLeftRight.Y = -1;
            }
        }
        
        // Bottom
        if (GetTile(X, Y + 1, out tile, out tilePrio)) {
            if (tilePrio > currPrio) {
                _blendTopBottom.Z = tile._blendUV.X;
                _blendTopBottom.W = tile._blendUV.Y;
                tile._blendTopBottom.X = -1;
                tile._blendTopBottom.Y = -1;
            } else if (currPrio > tilePrio) {
                _blendTopBottom.Z = -1;
                _blendTopBottom.W = -1;
                tile._blendTopBottom.X = _blendUV.X;
                tile._blendTopBottom.Y = _blendUV.Y;
            } else {
                _blendTopBottom.Z = -1;
                _blendTopBottom.W = -1;
                tile._blendTopBottom.X = -1;
                tile._blendTopBottom.Y = -1;
            }
        }
        
        // Left
        if (GetTile(X - 1, Y, out tile, out tilePrio)) {
            if (tilePrio > currPrio) {
                _blendLeftRight.X = tile._blendUV.X;
                _blendLeftRight.Y = tile._blendUV.Y;
                tile._blendLeftRight.Z = -1;
                tile._blendLeftRight.W = -1;
            } else if (currPrio > tilePrio) {
                _blendLeftRight.X = -1;
                _blendLeftRight.Y = -1;
                tile._blendLeftRight.Z = _blendUV.X;
                tile._blendLeftRight.W = _blendUV.Y;
            } else {
                _blendLeftRight.X = -1;
                _blendLeftRight.Y = -1;
                tile._blendLeftRight.Z = -1;
                tile._blendLeftRight.W = -1;
            }
        }
    }

    private void UpdateCorners() {
        for (var x = X - 1; x <= X + 1; x++) {
            for (var y = Y - 1; y <= Y + 1; y++) {
                var tile = Map.GetTile(x, y);
                tile?.SetCornerBlends();
            }
        }
    }

    private void SetCornerBlends() {
        var currPrio = GroundProperties.BlendPriority;
        
        // Bottom Right
        if (GetTile(X + 1, Y + 1, out var tile, out var tilePrio) && GetTile(X + 1, Y, out var t1, out _) && GetTile(X, Y + 1, out var t2, out _)) {
            if (currPrio < tilePrio && CompareBlend(t1._blendTopBottom.Z, t2._blendLeftRight.Z) && CompareBlend(t1._blendTopBottom.W, t2._blendLeftRight.W)) {
                _cornerBottom.X = tile._blendUV.X;
                _cornerBottom.Y = tile._blendUV.Y;
            } else {
                _cornerBottom.X = -1;
                _cornerBottom.Y = -1;
            }
        }
        
        // Bottom Left
        if (GetTile(X - 1, Y + 1, out tile, out tilePrio) && GetTile(X - 1, Y, out t1, out _) && GetTile(X, Y + 1, out t2, out _)) {
            if (currPrio < tilePrio && CompareBlend(t1._blendTopBottom.Z, t2._blendLeftRight.X) && CompareBlend(t1._blendTopBottom.W, t2._blendLeftRight.Y)) {
                _cornerBottom.Z = tile._blendUV.X;
                _cornerBottom.W = tile._blendUV.Y;
            } else {
                _cornerBottom.Z = -1;
                _cornerBottom.W = -1;
            }
        }
        
        // Top Right
        if (GetTile(X + 1, Y - 1, out tile, out tilePrio) && GetTile(X + 1, Y, out t1, out _) && GetTile(X, Y - 1, out t2, out _)) {
            if (currPrio < tilePrio && CompareBlend(t1._blendTopBottom.X, t2._blendLeftRight.Z) && CompareBlend(t1._blendTopBottom.Y, t2._blendLeftRight.W)) {
                _cornerTop.X = tile._blendUV.X;
                _cornerTop.Y = tile._blendUV.Y;
            } else {
                _cornerTop.X = -1;
                _cornerTop.Y = -1;
            }
        }
        
        // Top Left
        if (GetTile(X - 1, Y - 1, out tile, out tilePrio) && GetTile(X - 1, Y, out t1, out _) && GetTile(X, Y - 1, out t2, out _)) {
            if (currPrio < tilePrio && CompareBlend(t1._blendTopBottom.X, t2._blendLeftRight.X) && CompareBlend(t1._blendTopBottom.Y, t2._blendLeftRight.Y)) {
                _cornerTop.Z = tile._blendUV.X;
                _cornerTop.W = tile._blendUV.Y;
            } else {
                _cornerTop.Z = -1;
                _cornerTop.W = -1;
            }
        }
    }

    // ReSharper disable CompareOfFloatsByEqualityOperator
    private static bool CompareBlend(float a, float b) {
        if (a == -1f) return false;
        if (b == -1f) return false;
        return a == b;
    }
    
    private bool GetTile(int x, int y, out MapTile mapTile, out int prio) {
        var tile = Map.GetTile(x, y);
        
        mapTile = tile;
        prio = tile?.GroundProperties.BlendPriority ?? 0;
        return tile != null;
    }
}