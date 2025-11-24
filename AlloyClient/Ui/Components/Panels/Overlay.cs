using AlloyClient.Display;
using AlloyClient.UiLib.Core;

namespace AlloyClient.Ui.Components.Panels;

public class Overlay : Sprite {

    public virtual bool InputBlocker => true;

    public virtual void CloseOverlay() => OverlayManager.CloseOverlay(this);
}