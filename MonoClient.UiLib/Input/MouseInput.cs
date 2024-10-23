using System;
using MonoClient.UiLib.Core.Events.Types;
using Common.Vector;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.Core;

namespace MonoClient.UiLib.Input;

public static class MouseInput {
    private static MouseState _prevInput;
    private static MouseState _currInput;

    public static void Update() {
        _prevInput = _currInput;
        _currInput = Mouse.GetState();
    }

    public static IntVector2 GetMousePosition() => new(_currInput.X, _currInput.Y);

    public static float GetScrollDelta() => _currInput.ScrollWheelValue - _prevInput.ScrollWheelValue;
    
    public static bool HandleEvent(MouseEventId eventId) {
        return eventId switch {
            MouseEventId.LeftClick => WasButtonUp(MouseEventId.LeftClick),
            MouseEventId.MiddleClick => WasButtonUp(MouseEventId.MiddleClick),
            MouseEventId.RightClick => WasButtonUp(MouseEventId.RightClick),
            MouseEventId.LeftDown => WasButtonDown(MouseEventId.LeftClick),
            MouseEventId.MiddleDown => WasButtonDown(MouseEventId.MiddleClick),
            MouseEventId.RightDown => WasButtonDown(MouseEventId.RightClick),
            MouseEventId.LeftUp => WasButtonUp(MouseEventId.LeftClick),
            MouseEventId.MiddleUp => WasButtonUp(MouseEventId.MiddleClick),
            MouseEventId.RightUp => WasButtonUp(MouseEventId.RightClick),
            _ => throw new NotSupportedException()
        };
    }
    
    public static bool WasButtonDown(MouseEventId eventId) {
        return GetMouseButtonState(_currInput, eventId, ButtonState.Pressed) && GetMouseButtonState(_prevInput, eventId, ButtonState.Released);
    }
    
    public static bool WasButtonUp(MouseEventId eventId) {
        return GetMouseButtonState(_currInput, eventId, ButtonState.Released) && GetMouseButtonState(_prevInput, eventId, ButtonState.Pressed);
    }

    private static bool GetMouseButtonState(MouseState input, MouseEventId eventId, ButtonState state) {
        return eventId switch {
            MouseEventId.LeftClick => input.LeftButton == state,
            MouseEventId.MiddleClick => input.MiddleButton == state,
            MouseEventId.RightClick => input.RightButton == state,
            _ => throw new ArgumentOutOfRangeException(nameof(eventId), eventId, null)
        };
        
    }
}

