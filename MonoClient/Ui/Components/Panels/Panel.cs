using MonoClient.UiLib.BuiltIn;

namespace MonoClient.Ui.Components.Panels;

public enum PanelState {
    Active = 0,
    Closed = 1,
    Finished = 2
}

public class Panel : DisplayContainer {
    public PanelState State = PanelState.Active;
    public bool BlocksInput { get; protected set; }
    public virtual void ClosePanel() => State = PanelState.Closed;

}