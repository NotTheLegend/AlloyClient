using AlloyClient.Game.Objects;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Game.Components.Hud.Inventory;

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
        _height = 148;

        if (owner == Map.LocalPlayer)
        {
            var bg = new CutEdgeRect(new CutEdgeConfig { Width = 223, Height = _height, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x242222 });
            AddChild(bg);

            var hpSlotOutline = new CutEdgeRect(new CutEdgeConfig { Width = 101, Height = 26, CutX = 4, CutY = 4, Cuts = CutEdges.Left, Color = 0x3e3d3d });
            hpSlotOutline.Y += 152 - 38;
            hpSlotOutline.X = 8;
            AddChild(hpSlotOutline);

            var hpSlot = new CutEdgeRect(new CutEdgeConfig { Width = 95, Height = 20, CutX = 4, CutY = 4, Cuts = CutEdges.Left, Color = 0x242222 });
            hpSlot.Y += 152 - 38 + 3;
            hpSlot.X = 8 + 3;
            AddChild(hpSlot);

            var mpSlotOutline = new CutEdgeRect(new CutEdgeConfig { Width = 101, Height = 26, CutX = 4, CutY = 4, Cuts = CutEdges.Right, Color = 0x3e3d3d });
            mpSlotOutline.Y += 152 - 38;
            mpSlotOutline.X = hpSlotOutline.X + 100 + 6;
            AddChild(mpSlotOutline);

            var mpSlot = new CutEdgeRect(new CutEdgeConfig { Width = 95, Height = 20, CutX = 4, CutY = 4, Cuts = CutEdges.Right, Color = 0x242222 });
            mpSlot.Y += 152 - 38 + 3;
            mpSlot.X = hpSlotOutline.X + 100 + 6 + 3;
            AddChild(mpSlot);
        }

        _owner.InventoryUpdate.Add(OnInventoryChange);

        for (var i = 0; i < NumSlots; i++)
        {
            var slot = new ItemTile(owner, (byte)(i + offset), _interactive, Cuts[i], oneWay, tileSize: 48);
            slot.SetTileNumber(i + 1);
            slot.X = i % 4 * (48 + 5) + 8;
            slot.Y = i / 4 * (48 + 5) + 8;
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

        if (slot < _offset || slot >= _offset + NumSlots) return;
        _tiles[slot - _offset].SetItem(_owner.Equipment[slot]);
    }
    
}