using System;
using Common;
using OpenTK.Mathematics;
using RealmClient.UiLib.Enums;

namespace RealmClient.UiLib.Utils;

internal static class InternalUtils {

    internal static void Hex(ref this Color color, uint rgb, float alpha = 1.0f) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        var a = (byte)(Math.Max(Math.Min(alpha, 1f), 0f) * byte.MaxValue);
        color.PackedValue = (uint)(a << 24 | b << 16 | g << 8 | r);
    }
    
    internal static (int, int) GetAnchorOffset(UiAnchor type, int w, int h) {
        return type switch {
            UiAnchor.LeftTop => (0, 0),
            UiAnchor.MiddleTop => (-w / 2, 0),
            UiAnchor.RightTop => (-w, 0),
            UiAnchor.MiddleLeft => (0, -h / 2),
            UiAnchor.Middle => (-w / 2, -h / 2),
            UiAnchor.MiddleRight => (-w, -h / 2),
            UiAnchor.LeftBottom => (0, -h),
            UiAnchor.MiddleBottom => (-w / 2, -h),
            UiAnchor.RightBottom => (-w, -h),
            _ => (0, 0)
        };
    }
}