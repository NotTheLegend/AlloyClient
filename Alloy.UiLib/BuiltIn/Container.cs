using Alloy.UiLib.Core;

namespace Alloy.UiLib.BuiltIn;

public struct ContainerConfig {
    public int X = 0;
    public int Y = 0;
    public UiAnchor Anchor = UiAnchor.None;

    public ContainerConfig() { }
}

public class Container : DisplayContainer {
    public Container() : this(new ContainerConfig{Anchor = UiAnchor.None}) { }
    
    public Container(ContainerConfig config) {
        X = config.X;
        Y = config.Y;
        Anchor = config.Anchor;
    }
}