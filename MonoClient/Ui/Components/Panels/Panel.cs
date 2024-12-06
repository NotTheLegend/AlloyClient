using MonoClient.Display;
using MonoClient.UiLib.Core;

namespace MonoClient.Ui.Components.Panels;

public class Panel : Sprite {

    public virtual bool InputBlocker => true;

    public virtual void ClosePanel() => PanelManager.ClosePanel(this);
}