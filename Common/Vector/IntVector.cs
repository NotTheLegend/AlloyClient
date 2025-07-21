using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Common.Vector;
//TODO: remove, opentk has one
public struct IntVector2 : IEquatable<IntVector2> {
    
    [DataMember] public int X;
    [DataMember] public int Y;

    public IntVector2(int x, int y) {
        X = x;
        Y = y;
    }

    public IntVector2(int value) {
        X = value;
        Y = value;
    }

    public override bool Equals(object obj) {
        return obj is IntVector2 other && Equals(other);
    }

    public bool Equals(IntVector2 other) {
        return X == other.X && Y == other.Y;
    }

    public static IntVector2 operator -(IntVector2 value) {
        value.X = -value.X;
        value.Y = -value.Y;
        return value;
    }

    public static IntVector2 operator +(IntVector2 value1, IntVector2 value2) {
        value1.X += value2.X;
        value1.Y += value2.Y;
        return value1;
    }

    public static IntVector2 operator -(IntVector2 value1, IntVector2 value2) {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        return value1;
    }

    public static IntVector2 operator *(IntVector2 value1, IntVector2 value2) {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        return value1;
    }

    public static IntVector2 operator *(IntVector2 value, float scaleFactor) {
        value.X = (int)(value.X * scaleFactor);
        value.Y = (int)(value.Y * scaleFactor);
        return value;
    }

    public static IntVector2 operator *(float scaleFactor, IntVector2 value) {
        value.X = (int)(value.X * scaleFactor);
        value.Y = (int)(value.Y * scaleFactor);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntVector2 operator /(IntVector2 value1, IntVector2 value2) {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        return value1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntVector2 operator /(IntVector2 value1, float divider) {
        var num = 1f / divider;
        value1.X = (int)(value1.X * num);
        value1.Y = (int)(value1.Y * num);
        return value1;
    }

    public static bool operator ==(IntVector2 value1, IntVector2 value2) {
        return value1.X == value2.X && value1.Y == value2.Y;
    }

    public static bool operator !=(IntVector2 value1, IntVector2 value2) {
        return value1.X != value2.X || value1.Y != value2.Y;
    }

    public static IntVector2 Add(IntVector2 value1, IntVector2 value2) {
        value1.X += value2.X;
        value1.Y += value2.Y;
        return value1;
    }

    public static void Add(ref IntVector2 value1, ref IntVector2 value2, out IntVector2 result) {
        result.X = value1.X + value2.X;
        result.Y = value1.Y + value2.Y;
    }

    public (int, int) ToPair() => (X, Y);

    public override int GetHashCode() {
        return (X.GetHashCode() * 397) ^ Y.GetHashCode();
    }

    public override string ToString() {
        return "{X:" + X + " Y:" + Y + "}";
    }

    public static IntVector2 Max(IntVector2 value1, IntVector2 value2) {
        value1.X = Math.Max(value1.X, value2.X);
        value1.Y = Math.Max(value1.Y, value2.Y);
        return value1;
    }
}