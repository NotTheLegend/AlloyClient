using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;

namespace RealmClient.UiLib.BuiltIn;

public struct ContainerConfig {
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public UiAnchor Anchor = UiAnchor.LeftTop;
    public bool EnableClip = false;

    public ContainerConfig() { }
}

public class Container : Sprite {
    public Container() : this(new ContainerConfig()) { }
    
    public Container(ContainerConfig config) {
        X = config.X;
        Y = config.Y;
        EnableClipRect = config.EnableClip;
        SetAnchor(config.Anchor);
        MouseEnabled = config.EnableClip;
    }
}