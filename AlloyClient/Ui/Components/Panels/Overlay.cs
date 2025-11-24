using RealmClient.UiLib.Core;
using RealmClient.Display;

namespace RealmClient.Ui.Components.Panels;

public class Overlay : Sprite {

    public virtual bool InputBlocker => true;

    public virtual void CloseOverlay() => OverlayManager.CloseOverlay(this);
}