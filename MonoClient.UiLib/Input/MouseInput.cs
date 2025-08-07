using System;
using System.Collections.Generic;
using MonoClient.UiLib.Core;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace MonoClient.UiLib.Input;

internal static class MouseInput {

    private static readonly string[] EventTypes = [
        MouseEvent.LeftDown, MouseEvent.MiddleDown, MouseEvent.RightDown,
        MouseEvent.LeftUp, MouseEvent.MiddleUp, MouseEvent.RightUp,
        MouseEvent.MouseMove, MouseEvent.ScrollVertical, MouseEvent.ScrollHorizontal
    ];

    internal static readonly Queue<string> Events = [];

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

    private static string ButtonToEvent(MouseButton button, bool down) => button switch {
        MouseButton.Button1 => down ? MouseEvent.LeftDown : MouseEvent.LeftUp,
        MouseButton.Button2 => down ? MouseEvent.RightDown : MouseEvent.RightUp,
        MouseButton.Button3 => down ? MouseEvent.MiddleDown : MouseEvent.MiddleUp,
        MouseButton.Button4 => "",
        MouseButton.Button5 => "",
        MouseButton.Button6 => "",
        MouseButton.Button7 => "",
        MouseButton.Button8 => "", //TODO: extra mouse buttons

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
    };


    internal static bool CheckEvent(string type) {
        return type switch {
            MouseEvent.LeftDown => WasButtonDown(MouseEvent.LeftClick),
            MouseEvent.MiddleDown => WasButtonDown(MouseEvent.MiddleClick),
            MouseEvent.RightDown => WasButtonDown(MouseEvent.RightClick),
            MouseEvent.LeftUp => WasButtonUp(MouseEvent.LeftClick),
            MouseEvent.MiddleUp => WasButtonUp(MouseEvent.MiddleClick),
            MouseEvent.RightUp => WasButtonUp(MouseEvent.RightClick),
            _ => throw new NotSupportedException()
        };
    }

    private static bool WasButtonDown(string eventId) {
        return GetMouseButtonState(_mouseState, eventId) && !GetMouseButtonState(_prevMouseState, eventId);
    }

    private static bool WasButtonUp(string eventId) {
        return GetMouseButtonState(_mouseState, eventId) && !GetMouseButtonState(_prevMouseState, eventId);
    }

    private static bool GetMouseButtonState(MouseButtonFlags input, string eventId) {
        return eventId switch {
            MouseEvent.LeftClick => (input & MouseButtonFlags.Button1) != 0,
            MouseEvent.MiddleClick => (input & MouseButtonFlags.Button3) != 0,
            MouseEvent.RightClick => (input & MouseButtonFlags.Button2) != 0,
            _ => throw new ArgumentOutOfRangeException(nameof(eventId), eventId, null)
        };

    }
}