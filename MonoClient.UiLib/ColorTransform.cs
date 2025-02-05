using Microsoft.Xna.Framework;

namespace MonoClient.UiLib;

public struct ColorTransform {
    public static ColorTransform Default = new(1f, 1f, 1f, 1f);
    public static ColorTransform Bright = new(20, 20, 20, 0);
    public static ColorTransform Bright2 = new(40, 40, 40, 0);
    public static ColorTransform VeryBlue = new(0.3f, 0.3f, 1, 1, 0, 0, 100, 0);
    public static ColorTransform Dark = new(0.6f, 0.6f, 0.6f, 1);
    public static ColorTransform Dim = new(0.4f, 0.4f, 0.4f, 1);
    
    // Stars
    public static ColorTransform HalfTransparent = new(1f, 1f, 1f, 0.5f);
    public static ColorTransform LightBlue = new(138 / 255f, 152 / 255f, 222 / 255f, 1f);
    public static ColorTransform DarkBlue = new(49 / 255f,77 / 255f,219 / 255f, 1f);
    public static ColorTransform Red = new(193 / 255f,39 / 255f,45 / 255f, 1f);
    public static ColorTransform Orange = new(247 / 255f,147 / 255f,30 / 255f, 1f);
    public static ColorTransform Yellow = new(255 / 255f,255 / 255f,0 / 255f, 1f);

    private Vector4 _mult = new Vector4(1f);
    private Vector4 _add = new Vector4(0f);

    public ColorTransform(float redMult, float greenMult, float blueMult, float alphaMult) : this(redMult, greenMult, blueMult, alphaMult, 0, 0, 0, 0) { }

    public ColorTransform(byte redOff, byte greenOff, byte blueOff, byte alphaOff) : this(1, 1, 1, 1, redOff, greenOff, blueOff, alphaOff) { }

    public ColorTransform(float redMult, float greenMult, float blueMult, float alphaMult, byte redOff, byte greenOff, byte blueOff, byte alphaOff) {
        _mult = new Vector4(redMult, greenMult, blueMult, alphaMult);
        _add = new Vector4(redOff, greenOff, blueOff, alphaOff);
    }
    
    public static ColorTransform operator *(ColorTransform value1, ColorTransform value2) {
        value1._mult *= value2._mult;
        value1._add += value2._add;
        return value1;
    }

    internal Vector4 GetTransformData() {
        return _mult + _add * 1000;
    }
}