using System;
using System.Collections.Generic;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Alloy.UiLib.Input;

internal static class MouseInput {

    private static readonly EventType<MouseEvent>[] EventTypes = [
        MouseEvent.LeftDown, MouseEvent.MiddleDown, MouseEvent.RightDown,
        MouseEvent.LeftUp, MouseEvent.MiddleUp, MouseEvent.RightUp,
        MouseEvent.MouseMove, MouseEvent.ScrollVertical, MouseEvent.ScrollHorizontal
    ];

    internal static readonly Queue<EventType<MouseEvent>> Events = [];

    private static MouseButtonFlags _prevMouseState;
    private static MouseButtonFlags _mouseState;
    private static Vector2 _scrollDelta;
    private static Vector2 _mousePosition;

    internal static void Update() {
        _prevMouseState = _mouseState;
    }

    internal static void SetKeyDown(MouseButton button) {
        var me = ButtonToEvent(button, true);

        if (me == "") return;
        
        _mouseState |= ButtonToFlag(button);
        Events.Enqueue(me);
    }

    internal static void SetKeyUp(MouseButton button) {
        var me = ButtonToEvent(button, false);

        if (me == "") return;
        
        _mouseState &= ~ButtonToFlag(button);
        Events.Enqueue(me);
    }

    internal static void SetScrollDelta(Vector2 delta) {
        _scrollDelta = delta;
        if (_scrollDelta.X != 0) Events.Enqueue(MouseEvent.ScrollHorizontal);
        if (_scrollDelta.Y != 0) Events.Enqueue(MouseEvent.ScrollVertical);
    }

    internal static void SetMousePosition(Vector2 pos) {
        _mousePosition = pos;
        Events.Enqueue(MouseEvent.MouseMove);
    }

    internal static float GetVerticalScrollDelta() => _scrollDelta.Y;

    internal static float GetHorizontalScrollDelta() => _scrollDelta.X;

    internal static Vector2i GetMousePosition() => new((int) _mousePosition.X, (int)_mousePosition.Y);

    private static EventType<MouseEvent> ButtonToEvent(MouseButton button, bool down) => button switch {
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

    private static MouseButtonFlags ButtonToFlag(MouseButton button) => button switch {
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
}