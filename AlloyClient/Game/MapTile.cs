using System;
using System.Collections.Generic;
using System.Linq;
using AlloyClient.Assets;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Engine.Common;
using AlloyClient.Game.Components;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public class MapTile(int x, int y) {

    private const ushort DefaultTile = 0xff;
    
    private static readonly Vector4[] BlendMasks = Main.Atlas.GetAtlasData("tileAlphaBlend").Select(x => x.ToVector4(true)).ToArray();
    
    private enum BlendInfo : byte {
        Left,
        Right,
        Top,
        Bottom,
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight,
    }
    
    public readonly int X = x;
    public readonly int Y = y;

    public ushort Type = DefaultTile;
    public GroundProperties GroundProperties = GroundLibrary.TypeToGroundProps[DefaultTile];
    public TextureData TextureData = GroundLibrary.TypeToTextureData[DefaultTile];

    public Entity OccupiedObject { 
        get;
        set => SetMinimapColor(field = value);
    }

    private readonly SortedList<BlendInfo, TileData> _blends = new(8);

    private Color _color;
    private TileData _data;

    private void SetMinimapColor(Entity entity) {
        if (entity != null && entity.Properties.Static && entity.Properties.OccupySquare && !entity.Properties.NoMiniMap) {
            MinimapTexture.UncoverTile(X, Y, entity.GetDominateColor());
        } else {
            MinimapTexture.UncoverTile(X, Y, _color);
        }
    }

    public void DrawTile() {
        Render.DrawNewTile(_data);
        foreach (var data in _blends) {
            Render.DrawNewTile(data.Value);
        }
    }

    public void SetType(ushort type) {
        Type = type;
        GroundProperties = GroundLibrary.TypeToGroundProps[type];
        TextureData = GroundLibrary.TypeToTextureData[type];

        var texture = TextureData.GetTexture(out _color, true);
        texture.RemovePadding();
        
        SetMinimapColor(OccupiedObject);

        var offx = GroundProperties.XOffset;
        var offy = GroundProperties.YOffset;

        if (GroundProperties.RandomOffset) {
            offx = (int)(Random.Shared.NextSingle() * texture.RawW()) / (float)texture.RawW();
            offy = (int)(Random.Shared.NextSingle() * texture.RawH()) / (float)texture.RawH();
        }

        var animate = new Vector4(0);
        var animateProp = GroundProperties.Animate;
        switch (animateProp.Type) {
            case GroundAnimate.State.Wave:
                animate.X = animateProp.DeltaX;
                animate.Y = animateProp.DeltaY;
                break;
            case GroundAnimate.State.Flow:
                animate.Z = animateProp.DeltaX;
                animate.W = animateProp.DeltaY;
                break;
        }

        _data = new TileData(new Vector4(X, Y, offx, offy), texture.ToVector4(), animate, new Vector4(-1));

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
                var data = tile._data;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[3];
                _blends[BlendInfo.Top] = data;
                tile._blends.Remove(BlendInfo.Bottom);
            } else if (currPrio > tilePrio) {
                _blends.Remove(BlendInfo.Top);
                var data = _data;
                data.Position.X = tile.X;
                data.Position.Y = tile.Y;
                data.Mask = BlendMasks[2];
                tile._blends[BlendInfo.Bottom] = data;
            } else {
                _blends.Remove(BlendInfo.Top);
                tile._blends.Remove(BlendInfo.Bottom);
            }
        }
        
        // Right
        if (GetTile(X + 1, Y, out tile, out tilePrio)) {
            if (tilePrio > currPrio) {
                var data = tile._data;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[1];
                _blends[BlendInfo.Right] = data;
                tile._blends.Remove(BlendInfo.Left);
            } else if (currPrio > tilePrio) {
                _blends.Remove(BlendInfo.Right);
                var data = _data;
                data.Position.X = tile.X;
                data.Position.Y = tile.Y;
                data.Mask = BlendMasks[0];
                tile._blends[BlendInfo.Left] = data;
            } else {
                _blends.Remove(BlendInfo.Right);
                tile._blends.Remove(BlendInfo.Left);
            }
        }
        
        // Bottom
        if (GetTile(X, Y + 1, out tile, out tilePrio)) {
            if (tilePrio > currPrio) {
                var data = tile._data;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[2];
                _blends[BlendInfo.Bottom] = data;
                tile._blends.Remove(BlendInfo.Top);
            } else if (currPrio > tilePrio) {
                _blends.Remove(BlendInfo.Bottom);
                var data = _data;
                data.Position.X = tile.X;
                data.Position.Y = tile.Y;
                data.Mask = BlendMasks[3];
                tile._blends[BlendInfo.Top] = data;
            } else {
                _blends.Remove(BlendInfo.Bottom);
                tile._blends.Remove(BlendInfo.Top);
            }
        }
        
        // Left
        if (GetTile(X - 1, Y, out tile, out tilePrio)) {
            if (tilePrio > currPrio) {
                var data = tile._data;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[0];
                _blends[BlendInfo.Left] = data;
                tile._blends.Remove(BlendInfo.Right);
            } else if (currPrio > tilePrio) {
                _blends.Remove(BlendInfo.Left);
                var data = _data;
                data.Position.X = tile.X;
                data.Position.Y = tile.Y;
                data.Mask = BlendMasks[1];
                tile._blends[BlendInfo.Right] = data;
            } else {
                _blends.Remove(BlendInfo.Left);
                tile._blends.Remove(BlendInfo.Right);
            }
        }
    }

    private void UpdateCorners() {
        for (var x = X - 1; x <= X + 1; x++) {
            for (var y = Y - 1; y <= Y + 1; y++) {
                var tile = Map.GetTile(x, y);
                tile?.SetCornerBlendsNew();
            }
        }
    }
    
    private void SetCornerBlendsNew() {
        var currPrio = GroundProperties.BlendPriority;
        
        // Bottom Right
        if (GetTile(X + 1, Y + 1, out _, out var tilePrio) && GetTile(X + 1, Y, out var t1, out _) && GetTile(X, Y + 1, out var t2, out _)) {
            if (currPrio < tilePrio && t1._blends.TryGetValue(BlendInfo.Bottom, out var b1) && t2._blends.TryGetValue(BlendInfo.Right, out var b2) && b1.UV == b2.UV) {
                var data = b1;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[4];
                _blends[BlendInfo.BottomRight] = data;
            } else {
                _blends.Remove(BlendInfo.BottomRight);
            }
        }
        
        // Bottom Left
        if (GetTile(X - 1, Y + 1, out _, out tilePrio) && GetTile(X - 1, Y, out t1, out _) && GetTile(X, Y + 1, out t2, out _)) {
            if (currPrio < tilePrio && t1._blends.TryGetValue(BlendInfo.Bottom, out var b1) && t2._blends.TryGetValue(BlendInfo.Left, out var b2) && b1.UV == b2.UV) {
                var data = b1;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[5];
                _blends[BlendInfo.BottomLeft] = data;
            } else {
                _blends.Remove(BlendInfo.BottomLeft);
            }
        }
        
        // Top Right
        if (GetTile(X + 1, Y - 1, out _, out tilePrio) && GetTile(X + 1, Y, out t1, out _) && GetTile(X, Y - 1, out t2, out _)) {
            if (currPrio < tilePrio && t1._blends.TryGetValue(BlendInfo.Top, out var b1) && t2._blends.TryGetValue(BlendInfo.Right, out var b2) && b1.UV == b2.UV) {
                var data = b1;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[6];
                _blends[BlendInfo.TopRight] = data;
            } else {
                _blends.Remove(BlendInfo.TopRight);
            }
        }
        
        // Top Left
        if (GetTile(X - 1, Y - 1, out _, out tilePrio) && GetTile(X - 1, Y, out t1, out _) && GetTile(X, Y - 1, out t2, out _)) {
            if (currPrio < tilePrio && t1._blends.TryGetValue(BlendInfo.Top, out var b1) && t2._blends.TryGetValue(BlendInfo.Left, out var b2) && b1.UV == b2.UV) {
                var data = b1;
                data.Position.X = X;
                data.Position.Y = Y;
                data.Mask = BlendMasks[7];
                _blends[BlendInfo.TopLeft] = data;
            } else {
                _blends.Remove(BlendInfo.TopLeft);
            }
        }
    }
    
    private static bool GetTile(int x, int y, out MapTile mapTile, out int prio) {
        var tile = Map.GetTile(x, y);
        
        mapTile = tile;
        prio = tile?.GroundProperties.BlendPriority ?? 0;
        return tile != null;
    }
}