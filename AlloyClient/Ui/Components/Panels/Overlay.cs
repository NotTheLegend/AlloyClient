using AlloyClient.Display;
using Alloy.UiLib.Core;

namespace AlloyClient.Ui.Components.Panels;

public class Overlay : Sprite {

    public virtual bool InputBlocker => true;

    public virtual void CloseOverlay() => OverlayManager.Clear();
}