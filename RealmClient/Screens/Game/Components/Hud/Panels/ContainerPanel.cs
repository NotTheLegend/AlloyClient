using RealmClient.Objects;
using RealmClient.Screens.Game.Components.Hud.Inventory;

namespace RealmClient.Screens.Game.Components.Hud.Panels;

public class ContainerPanel : Panel {

    public ContainerPanel(Entity entity, bool oneWay) {
        var grid = new InventoryGrid(entity, 0, oneWay, false);
        AddChild(grid);
    }
    
}