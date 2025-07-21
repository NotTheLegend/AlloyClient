using OpenTK.Mathematics;

namespace MonoClient.UiLib.Extra;

public struct ColorTransform {
    
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