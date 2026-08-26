using System;
using System.Runtime.InteropServices;
using Alloy.Common;
using AlloyClient.Assets;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game.Components;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.VertexData;
using Alloy.Common.Structs;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public class MapTile(Vector2i position) {

    public const int MaxTileData = 13;

    public readonly int X = position.X;
    public readonly int Y = position.Y;

    public ushort Type = Const.DefaultTile;
    public GroundProperties GroundProperties = GroundLibrary.TypeToGroundProps[Const.DefaultTile];
    public TextureData TextureData = GroundLibrary.TypeToTextureData[Const.DefaultTile];

    public Entity OccupiedObject { 
        get;
        set => SetMinimapColor(field = value);
    }

    private Color _color;

    private RenderData _data;
    private int _dataCount;

    private void SetMinimapColor(Entity entity) {
        if (entity != null && entity.Properties.Static && entity.Properties.OccupySquare && !entity.Properties.NoMiniMap) {
            MinimapTexture.UncoverTile(X, Y, entity.GetDominateColor());
        } else {
            MinimapTexture.UncoverTile(X, Y, _color);
        }
    }

    public ReadOnlySpan<TileData> DrawTile() => _data.AsSpan(0, _dataCount);

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
        
        // can be shrunk down to 48 bytes by making posOff vec4short & animate vec4h
        _data[0] = new TileData(new Vector4(X, Y, offx, offy), texture.ToVector4(), animate, new Vector4(-1));
        _dataCount = 1;
    }

    public void Rebuild(Span<MapTile> tiles) {
        _dataCount = TileBuilder.Build(this, _data.AsSpan(1, MaxTileData - 1), tiles) + 1;
    }

    public TileData CloneWithBlend(int x, int y, Vector4 mask) {
        var tile = _data[0];
        tile.Position.X = x;
        tile.Position.Y = y;
        tile.Mask = mask;
        return tile;
    }
    
    public TileData CloneWithEdge(AtlasData uv, bool swizzle = false) {
        var tile = _data[0];
        tile.UV = uv.ToVector4();
        tile.Temp.X = swizzle ? 1 : 0;
        return tile;
    }

    public bool IsWalkable() {
        return !GroundProperties.NoWalk && (OccupiedObject == null || !OccupiedObject.Properties.OccupySquare);
    }
    
    [System.Runtime.CompilerServices.InlineArray(MaxTileData)]
    private struct RenderData {
        private TileData _;

        public Span<TileData> AsSpan() => MemoryMarshal.CreateSpan(ref _, MaxTileData);
        
        public Span<TileData> AsSpan(int offset, int length) => MemoryMarshal.CreateSpan(ref _, MaxTileData).Slice(offset, length);
    }
}
