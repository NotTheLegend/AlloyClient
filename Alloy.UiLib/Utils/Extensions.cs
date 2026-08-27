using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Alloy.UiLib.Core;
using OpenTK.Platform;

namespace Alloy.UiLib.Utils;

public static class Extensions {
    extension(MouseButton button) {
        internal MouseButtonFlags AsFlag() => button switch {
            MouseButton.Button1 => MouseButtonFlags.Button1,
            MouseButton.Button2 => MouseButtonFlags.Button2,
            MouseButton.Button3 => MouseButtonFlags.Button3,
            MouseButton.Button4 => MouseButtonFlags.Button4,
            MouseButton.Button5 => MouseButtonFlags.Button5,
            MouseButton.Button6 => MouseButtonFlags.Button6,
            MouseButton.Button7 => MouseButtonFlags.Button7,
            MouseButton.Button8 => MouseButtonFlags.Button8,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null),
        };

        internal EventType<MouseEvent> AsEventType(bool down) => button switch {
            MouseButton.Button1 => down ? MouseEvent.LeftDown : MouseEvent.LeftUp,
            MouseButton.Button2 => down ? MouseEvent.RightDown : MouseEvent.RightUp,
            MouseButton.Button3 => down ? MouseEvent.MiddleDown : MouseEvent.MiddleUp,
            MouseButton.Button4 => "",
            MouseButton.Button5 => "",
            MouseButton.Button6 => "",
            MouseButton.Button7 => "",
            MouseButton.Button8 => "", // TODO: extra mouse buttons
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null),
        };
    }

    extension(UiAnchor anchor) {
        internal (float, float) GetOffset(int w, int h) => anchor switch {
            UiAnchor.LeftTop => (0f, 0f),
            UiAnchor.MiddleTop => (-w / 2f, 0f),
            UiAnchor.RightTop => (-w, 0f),
            UiAnchor.MiddleLeft => (0f, -h / 2f),
            UiAnchor.Middle => (-w / 2f, -h / 2f),
            UiAnchor.MiddleRight => (-w, -h / 2f),
            UiAnchor.LeftBottom => (0f, -h),
            UiAnchor.MiddleBottom => (-w / 2f, -h),
            UiAnchor.RightBottom => (-w, -h),
            _ => (0f, 0f),
        };
    }

    extension<T>(List<T> list) {
        internal ReadOnlySpan<T> AsSpan() => CollectionsMarshal.AsSpan(list);
    }
}
