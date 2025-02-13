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

    private readonly int _height;

    private readonly bool _interactive;

    private bool _backpack;


    public InventoryGrid(Entity owner, int offset, bool oneWay = false, bool isBackpack = false) {
        _owner = owner;
        _offset = offset;
        _backpack = isBackpack;
        _interactive = owner == Map.LocalPlayer || owner.Properties.Container;
        _height = 150;

        if (owner == Map.LocalPlayer)
        {
            var bg = new CutEdgeRect(new CutEdgeConfig { Width = 224, Height = _height, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x242222 });
            AddChild(bg);

            var hpSlotOutline = new CutEdgeRect(new CutEdgeConfig { Width = 103, Height = 30, CutX = 4, CutY = 4, Cuts = CutEdges.Left, Color = 0x3e3d3d });
            hpSlotOutline.Y += 152 - 38;
            hpSlotOutline.X = 6;
            AddChild(hpSlotOutline);

            var hpSlot = new CutEdgeRect(new CutEdgeConfig { Width = 97, Height = 24, CutX = 4, CutY = 4, Cuts = CutEdges.Left, Color = 0x242222 });
            hpSlot.Y += 152 - 38 + 3;
            hpSlot.X = 6 + 3;
            AddChild(hpSlot);

            var mpSlotOutline = new CutEdgeRect(new CutEdgeConfig { Width = 103, Height = 30, CutX = 4, CutY = 4, Cuts = CutEdges.Right, Color = 0x3e3d3d });
            mpSlotOutline.Y += 152 - 38;
            mpSlotOutline.X = hpSlotOutline.X + 102 + 6;
            AddChild(mpSlotOutline);

            var mpSlot = new CutEdgeRect(new CutEdgeConfig { Width = 97, Height = 24, CutX = 4, CutY = 4, Cuts = CutEdges.Right, Color = 0x242222 });
            mpSlot.Y += 152 - 38 + 3;
            mpSlot.X = hpSlotOutline.X + 102 + 6 + 3;
            AddChild(mpSlot);
        }

        _owner.InventoryUpdate.Add(OnInventoryChange);

        for (var i = 0; i < NumSlots; i++)
        {
            var slot = new ItemTile(owner, (byte)(i + offset), _interactive, Cuts[i], oneWay, tileSize: 49);
            slot.SetTileNumber(i + 1);
            slot.X = i % 4 * (50 + 4) + 6;
            slot.Y = i / 4 * (50 + 4) + 6;
            AddChild(slot);
            _tiles[i] = slot;
        }
    }
    
    private void OnInventoryChange(int slot) 
    {
        if (!Visible) //Unsure how reliable this is, its to stop issues with the Backpack & Inventory trying to update when hidden
        {
            return;
        }

        Console.WriteLine($"New {slot} | {_offset} - {_offset + NumSlots} BackPack {_backpack} {Visible}");

        if (slot < _offset || slot >= _offset + NumSlots) return;
        _tiles[slot - _offset].SetItem(_owner.Equipment[slot]);
    }
    
}