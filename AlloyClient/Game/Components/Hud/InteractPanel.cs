using AlloyClient.Game.Components.Hud.Panels;
using AlloyClient.Game.Objects;
using AlloyClient.Game.Objects.Util;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Signals;

namespace AlloyClient.Game.Components.Hud;

public sealed class InteractPanel : Sprite {

    public static readonly SingleSignal<Panel> AddOverride = new();

    private Entity _currentObject;

    private Panel _currentPanel;

    private Panel _overridePanel;

    private readonly PartyPanel _partyPanel = new();

    public InteractPanel() {
        AddOverride.Set(SetOverride);
    }

    private void SetOverride(Panel panel) {
        if (_overridePanel != null)
            RemoveChild(_overridePanel);

        _currentPanel.Visible = false;
        _overridePanel = panel;
        AddChild(_overridePanel);
    }
    

    public void Update() {
        if (_overridePanel != null)
            return;

        var obj = EntityUtils.FindClosestEntityInRadius(Map.LocalPlayer, Map.InteractiveObjects.Values, 1f);

        if (obj == null) {
            _currentObject = null;
            SetPanel(_partyPanel);
            return;
        }

        if (obj == _currentObject && _currentPanel != null)
            return;
        
        _currentObject = obj;
        SetPanel(GetInteractPanel(_currentObject));
    }

    private void SetPanel(Panel panel) {
        RemoveChild(_currentPanel);
        _currentPanel = panel;

        if (_currentPanel == null)
            return;
        
        AddChild(_currentPanel);
    }
    
    public static bool IsInteractiveObject(Entity entity) {
        return entity.Properties.Class switch {
            "Container" => true,
            "OneWayContainer" => true,
            "Portal" => true,
            _ => false
        };
    }

    private static Panel GetInteractPanel(Entity entity) {
        if (entity == null)
            return null;
        
        return entity.Properties.Class switch {
            "Container" => new ContainerPanel(entity, false),
            "OneWayContainer" => new ContainerPanel(entity, true),
            "Portal" => new PortalPanel(entity),
            _ => null
        };
    }
}