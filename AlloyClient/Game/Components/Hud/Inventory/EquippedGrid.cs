using System.Collections.Generic;
using AlloyClient.Assets.XmlStructs;
using AlloyClient.Game.Objects;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Game.Components.Hud.Inventory;

public sealed class EquippedGrid : Sprite {

    private const byte NumSlots = 4;

    private readonly static CutEdges[] Cuts = [CutEdges.Left, CutEdges.None, CutEdges.None, CutEdges.Right];
    private readonly ItemTile[] _tileSlots = new ItemTile[4];

    private readonly Player _owner;

    public EquippedGrid(Player owner) {
        _owner = owner;
        
        var bg = new CutEdgeRect(new CutEdgeConfig { Width = 216, Height = 56, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x676767 });
        AddChild(bg);
        
        _owner.InventoryUpdate.Add(OnInventoryChange);

        for (byte i = 0; i < NumSlots; i++) {
            var slot = new ItemTile(_owner, i, true, Cuts[i], false, (byte)_owner.Properties.SlotTypes[i], tileSize: 49, bgcolor: 0x454545);
            slot.X = i % 4 * (49 + 4) + 4;
            slot.Y = i / 4 * (49 + 4) + 3;
            AddChild(slot);
            _tileSlots[i] = slot;
        }
    }
    
    public EquippedGrid(ItemDesc[] items, List<int> slotTypes) {
        var bg = new CutEdgeRect(new CutEdgeConfig { Width = 216, Height = 56, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x676767 });
        AddChild(bg);

        for (byte i = 0; i < NumSlots; i++) {
            var slot = new ItemTile(null, i, false, Cuts[i], false, (byte)slotTypes[i], tileSize: 49, bgcolor: 0x454545);
            slot.X = i % 4 * (49 + 4) + 4;
            slot.Y = i / 4 * (49 + 4) + 3;
            slot.SetItem(items[i]);
            AddChild(slot);
            _tileSlots[i] = slot;
        }
    }

    public void UpdateAbilitySlot() {
        var slot = _tileSlots[1];

        if (slot.ItemDesc == null || slot.ItemDesc.ObjectType == 0)
            return;

        var noMana = Map.LocalPlayer.Mp < slot.ItemDesc.MpCost;
        //todo: silence check
        
        slot.SetDim(noMana);
    }

    private void OnInventoryChange(int slot) {
        if (slot >= NumSlots) return;
        _tileSlots[slot].SetItem(_owner.Equipment[slot]);
    }
}