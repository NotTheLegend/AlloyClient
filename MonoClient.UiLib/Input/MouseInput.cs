using System;
using System.Collections.Generic;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.Core.Events;

namespace MonoClient.UiLib.Input;

internal static class MouseInput {
    
    private static Game _game;

    private static readonly string[] EventTypes = [
        MouseEvent.LeftDown, MouseEvent.MiddleDown, MouseEvent.RightDown, 
        MouseEvent.LeftUp, MouseEvent.MiddleUp, MouseEvent.RightUp, 
        MouseEvent.MouseMove, MouseEvent.Scroll];
    
    internal static readonly Queue<string> Events = [];
    
    private static MouseState _prevInput;
    private static MouseState _currInput;

    internal static void Register(Game game) {
        if (_game != null) return;

        _game = game;
    }

    internal static void Update() {
        if (!_game.IsActive)
            return;

        _prevInput = _currInput;
        _currInput = Mouse.GetState();

        if (_currInput.X < 0 || _currInput.X > UiRender.Screen.X || _currInput.Y < 0 || _currInput.Y > UiRender.Screen.Y)
            return;

        foreach (var type in EventTypes) {
            if (CheckEvent(type)) {
                Events.Enqueue(type);
            }
        }
    }

    internal static float GetScrollDelta() => _currInput.ScrollWheelValue - _prevInput.ScrollWheelValue;
    
    internal static IntVector2 GetMousePosition() => new(_currInput.X, _currInput.Y);

    internal static bool CheckEvent(string type) {
        return type switch {
            MouseEvent.LeftDown => WasButtonDown(MouseEvent.LeftClick),
            MouseEvent.MiddleDown => WasButtonDown(MouseEvent.MiddleClick),
            MouseEvent.RightDown => WasButtonDown(MouseEvent.RightClick),
            MouseEvent.LeftUp => WasButtonUp(MouseEvent.LeftClick),
            MouseEvent.MiddleUp => WasButtonUp(MouseEvent.MiddleClick),
            MouseEvent.RightUp => WasButtonUp(MouseEvent.RightClick),
            MouseEvent.Scroll => _currInput.ScrollWheelValue != _prevInput.ScrollWheelValue,
            MouseEvent.MouseMove => _currInput.Position != _prevInput.Position,
            _ => throw new NotSupportedException()
        };
    }
    
    private static bool WasButtonDown(string eventId) {
        return GetMouseButtonState(_currInput, eventId, ButtonState.Pressed) && GetMouseButtonState(_prevInput, eventId, ButtonState.Released);
    }
    
    private static bool WasButtonUp(string eventId) {
        return GetMouseButtonState(_currInput, eventId, ButtonState.Released) && GetMouseButtonState(_prevInput, eventId, ButtonState.Pressed);
    }

    private static bool GetMouseButtonState(MouseState input, string eventId, ButtonState state) {
        return eventId switch {
            MouseEvent.LeftClick => input.LeftButton == state,
            MouseEvent.MiddleClick => input.MiddleButton == state,
            MouseEvent.RightClick => input.RightButton == state,
            _ => throw new ArgumentOutOfRangeException(nameof(eventId), eventId, null)
        };
        
    }
}