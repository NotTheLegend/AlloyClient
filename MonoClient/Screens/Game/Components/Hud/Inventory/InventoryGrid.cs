using System;
using MonoClient.Objects;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud.Inventory;

public sealed class InventoryGrid : Sprite {

    private const int NumSlots = 8;

    private static readonly CutEdges[] Cuts = [CutEdges.TopLeft, CutEdges.None, CutEdges.None, CutEdges.TopRight, CutEdges.BottomLeft, CutEdges.None, CutEdges.None, CutEdges.BottomRight];
    private readonly ItemTile[] _tiles = new ItemTile[NumSlots];

    private readonly Entity _owner;

    private readonly int _offset;

    private readonly bool _interactive;

    public InventoryGrid(Entity owner, int offset, bool oneWay = false) {
        _owner = owner;
        _offset = offset;
        _interactive = owner == Map.LocalPlayer || owner.Properties.Container;
        
        //todo: one way chest logic
        
        var bg = new CutEdgeRect(new CutEdgeConfig { Width = 218, Height = 110, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x676767 });
        AddChild(bg);
        
        _owner.InventoryUpdate.Add(OnInventoryChange);

        for (var i = 0; i < NumSlots; i++) {
            var slot = new ItemTile(owner, (byte)(i + offset), _interactive, Cuts[i]);
            slot.SetTileNumber(i + 1);
            slot.X = i % 4 * (50 + 4) + 3;
            slot.Y = i / 4 * (50 + 4) + 3;
            AddChild(slot);
            _tiles[i] = slot;
        }
        
    }
    
    private void OnInventoryChange(int slot) {
        Console.WriteLine($"{slot} | {_offset} - {_offset + NumSlots}");
        if (slot < _offset || slot >= _offset + NumSlots) return;
        _tiles[slot - _offset].SetItem(_owner.Equipment[slot]);
    }
    
}