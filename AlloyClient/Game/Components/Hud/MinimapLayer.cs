using Common;
using OpenTK.Mathematics;
using RealmClient.Game.Objects;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Rendering;

namespace RealmClient.Game.Components.Hud;

public sealed class MinimapLayer : Container {

    private const int MaxEntities = 1000;
    private const int VertexSize = MaxEntities * 4;
    private const int IndexSize = MaxEntities * 6;

    private int _count = 0;
    private float _size;

    private static Entity _focus;

    public MinimapLayer() : base(new ContainerConfig { EnableClip = true }) {
        //todo:SetBaseDimensions(Minimap.MapSize, Minimap.MapSize);
        TextureId = TextureType.Color;
        
        AddEventListener(Event.EnterFrame, OnFrameEnter);

        ResizeBackBuffer();
    }

    private void ResizeBackBuffer() {
        VertexData = new VertexUi[VertexSize];
        Indices = new ushort[IndexSize];
        OverridePrimCount = 0;
    }

    public static void SetFocus(Entity entity) {
        _focus = entity;
    }

    public void SetSize(float size) => _size = size;

    private void AddObject(Vector2 pos, uint rgb) {
        if (_count >= MaxEntities) return;

        const float size = 3.25f;

        var color = Color.FromHexRGB(rgb);
        VertexData[_count * 4 + 0] = new VertexUi(new Vector2(pos.X - size, pos.Y - size), color);
        VertexData[_count * 4 + 1] = new VertexUi(new Vector2(pos.X + size, pos.Y - size), color);
        VertexData[_count * 4 + 2] = new VertexUi(new Vector2(pos.X + size, pos.Y + size), color);
        VertexData[_count * 4 + 3] = new VertexUi(new Vector2(pos.X - size, pos.Y + size), color);

        Indices[_count * 6 + 0] = (ushort)(_count * 4 + 0);
        Indices[_count * 6 + 1] = (ushort)(_count * 4 + 1);
        Indices[_count * 6 + 2] = (ushort)(_count * 4 + 2);
        Indices[_count * 6 + 3] = (ushort)(_count * 4 + 0);
        Indices[_count * 6 + 4] = (ushort)(_count * 4 + 2);
        Indices[_count * 6 + 5] = (ushort)(_count * 4 + 3);

        _count++;
        OverridePrimCount += 2;
    }

    private void OnFrameEnter() {
        if (Map.LocalPlayer == null) return;
        
        _count = 0;
        OverridePrimCount = 0;

        foreach (var kvp in Map.Entities) {
            var entity = kvp.Value;
            
            if (entity.Properties.Static || entity.Properties.NoMiniMap || entity.ObjectId == _focus.ObjectId) continue;

            var fillColor = 0u;
            
            if (entity is Player player) {
                if (false) {// todo: paused
                    fillColor = 0x7F7F7F;
                } else if (player.IsFellowGuild) {
                    fillColor = 0x00FF00;
                } else {
                    fillColor = 0xFFFF00;
                }
            } else {
                if (entity.Properties.IsEnemy) {
                    fillColor = 0xFF0000;
                } else if (entity.Properties.Class is "Portal" or "GuildHallPortal") {
                    fillColor = 0x0000FF;
                } else {
                    continue;
                }
            }

            var ratio = (entity.Position - Map.LocalPlayer.Position) / _size;

            var pos = new Vector2(Minimap.MapSize / 2f) + new Vector2(Minimap.MapSize / 2f) * ratio;
            AddObject(pos, fillColor);
        }
        
    }
}