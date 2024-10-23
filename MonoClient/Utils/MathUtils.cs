using System;
using Microsoft.Xna.Framework;

namespace MonoClient.Utils;

public class MathUtils {
    private static readonly Random Random = new();

    public static Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angle) {
        var cosTheta = (float)Math.Cos(angle);
        var sinTheta = (float)Math.Sin(angle);

        var x = cosTheta * (point.X - pivot.X) - sinTheta * (point.Y - pivot.Y) + pivot.X;
        var y = sinTheta * (point.X - pivot.X) + cosTheta * (point.Y - pivot.Y) + pivot.Y;

        return new Vector2(x, y);
    }

    public static int RandomInt(int max) => Random.Next(max);

    public static int RandomInt(int min, int max) => Random.Next(min, max);
}