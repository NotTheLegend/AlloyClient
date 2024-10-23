using Microsoft.Xna.Framework;

namespace MonoClient.Utils;

public static class ColorUtils {

    public static Color ColorHex(uint rgb) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        const byte a = 255;
        return new Color((uint) (a << 24 | b << 16 | g << 8 | r));
    }
    
}