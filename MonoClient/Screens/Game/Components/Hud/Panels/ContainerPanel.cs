using MonoClient.Objects;
using MonoClient.Screens.Game.Components.Hud.Inventory;

namespace MonoClient.Screens.Game.Components.Hud.Panels;

public class ContainerPanel : Panel {

    public ContainerPanel(Entity entity, bool oneWay) {
        var grid = new InventoryGrid(entity, 0, oneWay);
        AddChild(grid);
    }
    
}