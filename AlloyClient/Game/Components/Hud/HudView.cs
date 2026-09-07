using AlloyClient.Game.Components.Hud.Inventory;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Game.Components.Hud;

public sealed class HudView : Sprite {
    public const int HudWidth = 240;


    private Minimap _minimap;
    private CharacterDetails _details;
    private CharacterBars _bars;

    private EquippedGrid _equippedGrid;
    private InventoryGrid _inventoryGrid;
    private TabStrip _tabStrip;

    private InteractPanel _interactPanel;

    public HudView() {
        Anchor = UiAnchor.MiddleRight;

        var bg = new ColorRect(new ColorRectConfig {Width = HudWidth, Height = Settings.DefaultScreenHeight, Color = 0x363636});
        AddChild(bg);

        _minimap = new Minimap();
        _minimap.X = 5;
        _minimap.Y = 5;
        AddChild(_minimap);

        _details = new CharacterDetails();
        _details.X = 5;
        _details.Y = _minimap.Y + _minimap.Height;
        AddChild(_details);

        _bars = new CharacterBars();
        _bars.X = 15;
        _bars.Y = _details.Y + _details.Height + 5;
        AddChild(_bars);
    }

    public void CreatePlayerDependentAssets() {
        RemoveChild(_equippedGrid);
        RemoveChild(_inventoryGrid);
        RemoveChild(_interactPanel);
        RemoveChild(_tabStrip);

        _equippedGrid = new EquippedGrid(Map.LocalPlayer);
        _equippedGrid.X = 12;
        _equippedGrid.Y = _bars.Y + _bars.height * 3 + _bars.offset * 3;
        AddChild(_equippedGrid);

        _tabStrip = new TabStrip(Map.LocalPlayer); //Inv + Backpack + StatView is handled under TabStrip.
        _tabStrip.X = 8;
        _tabStrip.Y = _equippedGrid.Y + _equippedGrid.Height + 32;
        AddChild(_tabStrip);

        _interactPanel = new InteractPanel();
        _interactPanel.X = 8;
        _interactPanel.Y = Settings.DefaultScreenHeight - 115;
        AddChild(_interactPanel);
    }

    public void Update() {
        if (Map.LocalPlayer == null) {
            return;
        }

        _bars.Update();
        _equippedGrid.UpdateAbilitySlot();
        _interactPanel.Update();
    }
}