using RealmClient.UiLib.Extra;

namespace RealmClient.Utils;

public static class Transforms {
    public static readonly ColorTransform Default = new(1f, 1f, 1f, 1f);
    public static readonly ColorTransform Bright = new(20, 20, 20, 0);
    public static readonly ColorTransform Bright2 = new(40, 40, 40, 0);
    public static readonly ColorTransform VeryBlue = new(0.3f, 0.3f, 1, 1, 0, 0, 100, 0);
    public static readonly ColorTransform Dark = new(0.6f, 0.6f, 0.6f, 1);
    public static readonly ColorTransform Dim = new(0.4f, 0.4f, 0.4f, 1);
    
    // Stars
    public static readonly ColorTransform HalfTransparent = new(1f, 1f, 1f, 0.5f);
    public static readonly ColorTransform LightBlue = new(138 / 255f, 152 / 255f, 222 / 255f, 1f);
    public static readonly ColorTransform DarkBlue = new(49 / 255f,77 / 255f,219 / 255f, 1f);
    public static readonly ColorTransform Red = new(193 / 255f,39 / 255f,45 / 255f, 1f);
    public static readonly ColorTransform Orange = new(247 / 255f,147 / 255f,30 / 255f, 1f);
    public static readonly ColorTransform Yellow = new(255 / 255f,255 / 255f,0 / 255f, 1f);
}