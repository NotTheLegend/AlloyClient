using MonoClient.Objects.Util;
using MonoClient.Screens.Game.Components.Hud.Inventory;
using MonoClient.State;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud;

public sealed class HudView : Sprite {

    private bool _init = false;

    private Minimap _minimap;
    private CharacterDetails _details;
    private CharacterBars _bars;

    private EquippedGrid _equippedGrid;
    private InventoryGrid _inventoryGrid;
    
    private ContainerGrid _containerGrid;

    public HudView() {
        SetAnchor(UiAnchor.RightTop);
    }
    
    public void Create() {
        var bg = new ColorRect(new ColorRectConfig { Width = 256, Height = Settings.DefaultScreenHeight, Color = 0x363636 });
        AddChild(bg);

        _minimap = new Minimap();
        _minimap.X = 5;
        _minimap.Y = 5;
        AddChild(_minimap);

        _details = new CharacterDetails();
        _details.X = 5;
        _details.Y = _minimap.Y + _minimap.Height + 10;
        AddChild(_details);

        _bars = new CharacterBars();
        _bars.X = 10;
        _bars.Y = _details.Y + _details.Height + 10;
        AddChild(_bars);

        _equippedGrid = new EquippedGrid(Map.LocalPlayer);
        _equippedGrid.X = 10;
        _equippedGrid.Y = _bars.Y + _bars.Height * 4 + 10;
        AddChild(_equippedGrid);

        _inventoryGrid = new InventoryGrid(Map.LocalPlayer, 4);
        _inventoryGrid.X = 10;
        _inventoryGrid.Y = _equippedGrid.Y + _equippedGrid.Height + 5;
        AddChild(_inventoryGrid);
        
        _containerGrid = new ContainerGrid() {
            X = 10,
            Y = _bars.Y + _bars.Height * 9 + 10,
            Visible = false
        };
        //ContainerGrid.Initialized = false;
        //AddChild(_containerGrid);
    }

    public void Update() {
        if (!_init) return;
        
        _bars.Update();
        _equippedGrid.UpdateAbilitySlot();
        ManageInventory();

        var closestEntity = EntityUtils.FindClosestSpecialInRadius(Map.LocalPlayer, Map.Entities.Values, 1f);
        if (closestEntity == null) {
            _containerGrid.Visible = false;
            return;
        }

        // need to re-create instead of hiding ideally
        //switch (closestEntity.Properties.Class) {
        //    case "Container":
        //        _containerGrid.SetOwner(closestEntity);
        //        _containerGrid.Visible = true;
        //        break;
        //}
    }

    private void ManageInventory() {
       /* if (!ContainerGrid.Initialized) {
            _containerGrid.CreateGrid();
        }
        _containerGrid.Update();
        
        if (_containerGrid.Dragging)
            PrioritizeChild(_containerGrid);*/
    }
}