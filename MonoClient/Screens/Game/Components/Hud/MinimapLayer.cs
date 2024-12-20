using System;
using Microsoft.Xna.Framework;
using MonoClient.Objects;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;

namespace MonoClient.Screens.Game.Components.Hud;

public sealed class MinimapLayer : Container {

    private const int MaxEntities = 1000;
    private const int VertexSize = MaxEntities * 4;
    private const int IndexSize = MaxEntities * 6;

    private int _count = 0;
    private float _size;

    public MinimapLayer() : base(new ContainerConfig { EnableClip = true }) {
        SetBaseDimensions(Minimap.MapSize, Minimap.MapSize);
        TextureId = TextureType.Color;

        ResizeBackBuffer();
    }

    protected override void ResizeBackBuffer() {
        VertexData = new VertexUi[VertexSize];
        Indices = new short[IndexSize];
        base.ResizeBackBuffer();
        OverridePrimCount = 0;
    }

    public void SetSize(float size) => _size = size;

    private void AddObject(Vector2 pos, uint rgb) {
        if (_count >= MaxEntities) return;
        
        var color = rgb.ToColor();
        VertexData[_count * 4 + 0] = new VertexUi(new Vector2(pos.X - 2, pos.Y - 2), color);
        VertexData[_count * 4 + 1] = new VertexUi(new Vector2(pos.X + 2, pos.Y - 2), color);
        VertexData[_count * 4 + 2] = new VertexUi(new Vector2(pos.X + 2, pos.Y + 2), color);
        VertexData[_count * 4 + 3] = new VertexUi(new Vector2(pos.X - 2, pos.Y + 2), color);

        Indices[_count * 6 + 0] = (short)(_count * 4 + 0);
        Indices[_count * 6 + 1] = (short)(_count * 4 + 1);
        Indices[_count * 6 + 2] = (short)(_count * 4 + 2);
        Indices[_count * 6 + 3] = (short)(_count * 4 + 0);
        Indices[_count * 6 + 4] = (short)(_count * 4 + 2);
        Indices[_count * 6 + 5] = (short)(_count * 4 + 3);

        _count++;
        OverridePrimCount += 2;
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (Map.LocalPlayer == null) return;
        
        _count = 0;
        OverridePrimCount = 0;

        foreach (var kvp in Map.Entities) {
            var entity = kvp.Value;
            
            if (entity.Properties.Static) continue;

            var ratio = (entity.Position - Map.LocalPlayer.Position) / _size;

            var pos = new Vector2(Minimap.MapSize / 2f) + new Vector2(Minimap.MapSize / 2f) * ratio;
            var rgb = entity is not Player ? 0xFF0000u : 0xFFFF00u;
            AddObject(pos, rgb);
        }
        
    }
}