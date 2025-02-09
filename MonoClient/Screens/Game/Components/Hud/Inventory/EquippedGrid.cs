using System.Collections.Generic;
using MonoClient.Objects;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud.Inventory;

public sealed class EquippedGrid : Sprite {

    private const byte NumSlots = 4;

    private static readonly CutEdges[] Cuts = [CutEdges.Left, CutEdges.None, CutEdges.None, CutEdges.Right];
    private readonly ItemTile[] _tileSlots = new ItemTile[4];

    private readonly Player _owner;

    public EquippedGrid(Player owner) {
        _owner = owner;
        
        var bg = new CutEdgeRect(new CutEdgeConfig { Width = 218, Height = 56, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x676767 });
        AddChild(bg);
        
        _owner.InventoryUpdate.Add(OnInventoryChange);

        for (byte i = 0; i < NumSlots; i++) {
            var slot = new ItemTile(_owner, i, true, Cuts[i], false, (byte)_owner.Properties.SlotTypes[i]);
            slot.X = i % 4 * (50 + 4) + 3;
            slot.Y = i / 4 * (50 + 4) + 3;
            AddChild(slot);
            _tileSlots[i] = slot;
        }
    }
    
    public EquippedGrid(ItemDesc[] items, List<int> slotTypes) {
        var bg = new CutEdgeRect(new CutEdgeConfig { Width = 218, Height = 56, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x676767 });
        AddChild(bg);

        for (byte i = 0; i < NumSlots; i++) {
            var slot = new ItemTile(null, i, false, Cuts[i], false, (byte)slotTypes[i]);
            slot.X = i % 4 * (50 + 4) + 3;
            slot.Y = i / 4 * (50 + 4) + 3;
            slot.SetItem(items[i]);
            AddChild(slot);
            _tileSlots[i] = slot;
        }
    }

    public void UpdateAbilitySlot() {
        var slot = _tileSlots[1];

        if (slot.Item == null || slot.Item.ObjectType == 0)
            return;

        var noMana = Map.LocalPlayer.Mp < slot.Item.MpCost;
        //todo: silence check
        
        slot.SetDim(noMana);
    }

    private void OnInventoryChange(int slot) {
        if (slot >= NumSlots) return;
        _tileSlots[slot].SetItem(_owner.Equipment[slot]);
    }
}