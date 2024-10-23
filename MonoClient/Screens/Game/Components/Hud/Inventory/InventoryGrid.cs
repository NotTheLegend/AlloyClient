using System.Collections.Generic;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.UiLib.Core;

namespace MonoClient.Screens.Game.Components.Hud.Inventory;

public class InventoryGrid : Sprite {
    public static bool Initialized;
    public bool Dragging;
    
    public static List<InventoryTile> InventoryTiles;
    public ItemDesc[] CurrentEquipment = [];
    
    public void CreateGrid() {
        if (Map.LocalPlayer == null)
            return;
        
        CurrentEquipment = (ItemDesc[])Map.LocalPlayer.Equipment.Clone();
        InventoryTiles = [];
        
        // equipment
        for (var i = 0; i < 4; i++) {
            var tile = new InventoryTile(Map.LocalPlayer.Equipment[i]) {
                X = i * 52,
                Slot = (byte)i,
                Owner = Map.LocalPlayer
            };
            
            InventoryTiles.Add(tile);
            AddChild(tile);
        }
        
        // inventory
        for (int row = 0; row < 2; row++) {
            for (int col = 0; col < 4; col++) {
                int index = row * 4 + col;

                var tile = new InventoryTile(Map.LocalPlayer.Equipment[index + 4]) {
                    X = col * 52,
                    Y = 60 + row * 52,
                    Slot = (byte)(index + 4),
                    Owner = Map.LocalPlayer
                };

                InventoryTiles.Add(tile);
                AddChild(tile);
            }
        }

        Initialized = true;
    }

    public void Update() {
        if (!Initialized || Map.LocalPlayer == null)
            return;
        
        for (var i = 0; i < Map.LocalPlayer.Equipment.Length; i++) {
            if (CurrentEquipment[i] == Map.LocalPlayer.Equipment[i]) 
                continue;

            RefreshTiles();
            break;
        }
        
        if (InventoryTiles == null)
            return;
        
        foreach (var tile in InventoryTiles) {
            tile.Update();

            if (tile.Dragging) {
                Dragging = true;
                PrioritizeChild(tile);

                return;
            }

            Dragging = false;
        }
    }
    
    public void RefreshTiles() {
        InventoryTiles.Clear();
        InventoryTiles = null;
            
        RemoveAllChildren();
        CreateGrid();
    }
}