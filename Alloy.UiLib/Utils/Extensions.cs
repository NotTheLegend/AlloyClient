using System;
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
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };

        internal EventType<MouseEvent> AsEventType(bool down) => button switch {
            MouseButton.Button1 => down ? MouseEvent.LeftDown : MouseEvent.LeftUp,
            MouseButton.Button2 => down ? MouseEvent.RightDown : MouseEvent.RightUp,
            MouseButton.Button3 => down ? MouseEvent.MiddleDown : MouseEvent.MiddleUp,
            MouseButton.Button4 => "",
            MouseButton.Button5 => "",
            MouseButton.Button6 => "",
            MouseButton.Button7 => "",
            MouseButton.Button8 => "", //TODO: extra mouse buttons
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }

    extension(UiAnchor anchor) {
        internal (int, int) GetOffset(int w, int h) => anchor switch {
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