using System;
using Common;

namespace MonoClient.Utils;

public static class ColorUtils {

    public static Color ColorHex(uint rgb) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        const byte a = 255;
        return new Color((uint) (a << 24 | b << 16 | g << 8 | r));
    }

    public static Color ToColor(this uint rgb, float alpha = 1.0f) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        var a = (byte)(Math.Max(Math.Min(alpha, 1f), 0f) * byte.MaxValue);
        return new Color((uint)(a << 24 | b << 16 | g << 8 | r));
    }
    
}