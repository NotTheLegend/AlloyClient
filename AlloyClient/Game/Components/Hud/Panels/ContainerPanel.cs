using AlloyClient.Game.Components.Hud.Inventory;
using AlloyClient.Game.Objects;

namespace AlloyClient.Game.Components.Hud.Panels;

public class ContainerPanel : Panel {

    public ContainerPanel(Entity entity, bool oneWay) {
        var grid = new InventoryGrid(entity, 0, oneWay, false);
        AddChild(grid);
    }
    
}