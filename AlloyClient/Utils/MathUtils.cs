using System;
using OpenTK.Mathematics;

namespace AlloyClient.Utils;

public static class MathUtils {
    
    private static readonly Random Random = new();

    public static Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angle) {
        var cosTheta = (float)Math.Cos(angle);
        var sinTheta = (float)Math.Sin(angle);

        var x = cosTheta * (point.X - pivot.X) - sinTheta * (point.Y - pivot.Y) + pivot.X;
        var y = sinTheta * (point.X - pivot.X) + cosTheta * (point.Y - pivot.Y) + pivot.Y;

        return new Vector2(x, y);
    }
    
    // Normalizes an angle for the range -PI to PI
    public static float NormalizeAngle(float angle) {
        while (angle > MathF.PI) angle -= 2 * MathF.PI;
        while (angle < -MathF.PI) angle += 2 * MathF.PI;
        return angle;
    }

    public static float WrapAngle(float angle) {
        if ((angle > -MathHelper.Pi) && (angle <= MathHelper.Pi))
            return angle;
        angle %= MathHelper.TwoPi;
        if (angle <= -MathHelper.Pi)
            return angle + MathHelper.TwoPi;
        if (angle > MathHelper.Pi)
            return angle - MathHelper.TwoPi;
        return angle;
    }

    public static int RandomInt(int max) => Random.Next(max);

    public static int RandomInt(int min, int max) => Random.Next(min, max);

    public static float RandomPlusMinus(float range) => Random.NextSingle() * range * 2 - range;

    public static float GetDistanceSquared(Vector2 pos1, Vector2 pos2) {
        var x = pos2.X - pos1.X;
        var y = pos2.Y - pos1.Y;
        return x * x + y * y;
    }
    
    public static float Map(float value, float valMin, float valMax, float newMin, float newMax) {
        return (value - valMin) / (valMax - valMin) * (newMax - newMin) + newMin;
    }

    public static float BoundToPi(float num) {
        var v = 0;
        if (num < MathHelper.Pi) {
            v = ((int)(num / -MathHelper.Pi) + 1) / 2;
            num = num + v * 2 * MathHelper.Pi;
        } else if (num > MathHelper.Pi) {
            v = ((int)(num / MathHelper.Pi) + 1) / 2;
            num = num - v * 2 * MathHelper.Pi;
        }

        return num;
    }
}