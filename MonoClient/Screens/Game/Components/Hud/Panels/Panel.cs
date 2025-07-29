using MonoClient.UiLib.Core;
using MonoClient.UiLib.Signals;

namespace MonoClient.Screens.Game.Components.Hud.Panels;

public abstract class Panel : Sprite {

    public static readonly Signal OnInteract = new(); 
    
    protected Panel() {
        SetBaseDimensions(218, 110);
        AddEventListener(Event.AddedToStage, () => { OnInteract.Add(OnInteractKey); });
        AddEventListener(Event.RemovedToStage, () => { OnInteract.Remove(OnInteractKey); });
    }

    protected virtual void OnInteractKey() {
        
    }
}