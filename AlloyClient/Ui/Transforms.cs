using Alloy.UiLib.Extra;

namespace AlloyClient.Ui;

public static class Transforms {
    public readonly static ColorTransform Default = new(1f, 1f, 1f, 1f);
    public readonly static ColorTransform Bright = new(20, 20, 20, 0);
    public readonly static ColorTransform Bright2 = new(40, 40, 40, 0);
    public readonly static ColorTransform VeryBlue = new(0.3f, 0.3f, 1, 1, 0, 0, 100, 0);
    public readonly static ColorTransform Dark = new(0.6f, 0.6f, 0.6f, 1);
    public readonly static ColorTransform Dim = new(0.4f, 0.4f, 0.4f, 1);
    
    // Stars
    public readonly static ColorTransform HalfTransparent = new(1f, 1f, 1f, 0.5f);
    public readonly static ColorTransform LightBlue = new(138 / 255f, 152 / 255f, 222 / 255f, 1f);
    public readonly static ColorTransform DarkBlue = new(49 / 255f,77 / 255f,219 / 255f, 1f);
    public readonly static ColorTransform Red = new(193 / 255f,39 / 255f,45 / 255f, 1f);
    public readonly static ColorTransform Orange = new(247 / 255f,147 / 255f,30 / 255f, 1f);
    public readonly static ColorTransform Yellow = new(255 / 255f,255 / 255f,0 / 255f, 1f);
}