using RealmClient.UiLib.Core;

namespace RealmClient.Screens.Game.Components.Hud.Inventory;

public class ContainerGrid : Sprite {
    /*public Entity GridOwner;
    
    public static bool Initialized;
    public bool Dragging;
    
    public static List<InventoryTile> InventoryTiles;
    public ItemDesc[] CurrentEquipment = [];

    public ContainerGrid(Entity owner) {
        GridOwner = owner;
    }
    public void CreateGrid() {
        var active = GridOwner != null;
        
        CurrentEquipment = active ? (ItemDesc[])GridOwner.Equipment.Clone() : new ItemDesc[20];
        InventoryTiles = [];
        
        for (int row = 0; row < 2; row++) {
            for (int col = 0; col < 4; col++) {
                int index = row * 4 + col;

                var tile = new InventoryTile(active ? GridOwner.Equipment[index] : null) {
                    X = col * 52,
                    Y = 70 + row * 52,
                    Slot = (byte)index,
                    Owner = active ? GridOwner : null
                };

                InventoryTiles.Add(tile);
                AddChild(tile);
            }
        }

        Initialized = true;
    }

    public void Update() {
        if (!Initialized || GridOwner == null)
            return;
        
        for (var i = 0; i < GridOwner.Equipment.Length; i++) {
            if (CurrentEquipment[i] == GridOwner.Equipment[i]) 
                continue;

            RefreshTiles();
            break;
        }
        
        if (InventoryTiles == null)
            return;
        
        /*foreach (var tile in InventoryTiles) {

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
    
    public void SetOwner(Entity entity) {
        if (GridOwner != null && GridOwner.ObjectId != entity.ObjectId) {
            RefreshTiles();
        }
        else if (GridOwner == null) {
            RefreshTiles();
        }

        GridOwner = entity;
    }*/
}