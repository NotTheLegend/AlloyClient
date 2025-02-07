using MonoClient.Display;
using MonoClient.UiLib.Core;

namespace MonoClient.Ui.Components.Panels;

public class Overlay : Sprite {

    public virtual bool InputBlocker => true;

    public virtual void ClosePanel() => OverlayManager.CloseOverlay(this);
}