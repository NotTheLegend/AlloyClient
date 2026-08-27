using System;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Extra;

public struct ColorTransform : IEquatable<ColorTransform> {

    public readonly static ColorTransform Default = new(1f, 1f, 1f, 1f);

    private Vector4 _mult = new(1f);
    private Vector4 _add = new(0f);

    public ColorTransform(byte redOff, byte greenOff, byte blueOff, byte alphaOff) :
        this(1, 1, 1, 1, redOff, greenOff, blueOff, alphaOff) {
    }

    public ColorTransform(float redMult, float greenMult, float blueMult, float alphaMult, byte redOff = 0, byte greenOff = 0,
        byte blueOff = 0,
        byte alphaOff = 0) {
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

    public static implicit operator Vector4(ColorTransform transform) {
        return transform._mult + transform._add * 1000;
    }

    public bool Equals(ColorTransform other) {
        return _mult.Equals(other._mult) && _add.Equals(other._add);
    }

    public override bool Equals(object obj) {
        return obj is ColorTransform other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(_mult, _add);
    }

    public static bool operator ==(ColorTransform left, ColorTransform right) {
        return left.Equals(right);
    }

    public static bool operator !=(ColorTransform left, ColorTransform right) {
        return !(left == right);
    }
}