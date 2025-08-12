using RealmClient.UiLib.Core;
using RealmClient.UiLib.Signals;

namespace RealmClient.Screens.Game.Components.Hud.Panels;

public abstract class Panel : Sprite {

    public static readonly Signal OnInteract = new(); 
    
    protected Panel() {
        SetBaseDimensions(218, 110);
        AddEventListener(Event.AddedToStage, () => { OnInteract.Add(OnInteractKey); });
        AddEventListener(Event.RemovedFromStage, () => { OnInteract.Remove(OnInteractKey); });
    }

    protected virtual void OnInteractKey() {
        
    }
}