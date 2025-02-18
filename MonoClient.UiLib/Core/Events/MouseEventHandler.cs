using System;
using System.Collections.Generic;
using Common.Vector;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Input;

namespace MonoClient.UiLib.Core.Events;

internal class MouseEventHandler {
    private readonly Queue<EventData> _pending = new();

    private readonly List<MouseEventData> _mouseOut = [];
    private readonly List<MouseEventData> _callbacks = [];
    private readonly List<MouseEventData> _globalCallbacks = [];
    
    private bool _prevMouseInBounds;
    private IntVector2 _prevMousePosition;
    private IntVector2 _prevMousePositionGlobal;

    internal void UpdateQueue() {
        while (_pending.TryDequeue(out var pending)) {
            var list = pending.Data.EventId == MouseEventId.MouseOut ? _mouseOut : pending.Global ? _globalCallbacks : _callbacks;

            if (pending.Add) {
                list.Add(pending.Data);
            } else {
                list.Remove(pending.Data);
            }
        }
    }

    internal void HandleGlobal(Sprite sprite) {
        var pos = MouseInput.GetMousePosition();
        var delta = Math.Max(Math.Min(MouseInput.GetScrollDelta(), 1), -1);
        var inBounds = sprite.IsInBounds(pos);
        var shift = KeyboardInput.IsKeyDown(Keys.LeftShift) || KeyboardInput.IsKeyDown(Keys.RightShift);
        var ctrl = KeyboardInput.IsKeyDown(Keys.LeftControl) || KeyboardInput.IsKeyDown(Keys.RightControl);
        var alt = KeyboardInput.IsKeyDown(Keys.LeftAlt) || KeyboardInput.IsKeyDown(Keys.RightAlt);

        var args = new MouseEventArgs(sprite, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetScrollDelta(), 1), -1), shift, ctrl, alt);

        if (Sprite.HighestSprite == null && inBounds) {
            Sprite.HighestSprite = sprite;
        }
        
        var forceOut = Sprite.HighestSprite == null || Sprite.HighestSprite != sprite;

        if (forceOut && _prevMouseInBounds) {
            foreach (var data in _mouseOut) {
                _prevMouseInBounds = false;
                DoCallback(data.Callback, args);
            }
        }
        
        foreach (var data in _globalCallbacks) {
            var bounds = data.IgnoreBounds || inBounds;
            switch (data.EventId) {
                case MouseEventId.MouseMove when pos != _prevMousePositionGlobal:
                    _prevMousePositionGlobal = pos;
                    DoCallback(data.Callback, args);
                    break;
                case MouseEventId.Scroll when bounds:
                    if (delta == 0f) {
                        break;
                    }
                    
                    DoCallback(data.Callback, args);
                    break;
                case MouseEventId.LeftClick:
                case MouseEventId.MiddleClick:
                case MouseEventId.RightClick:
                case MouseEventId.LeftDown:
                case MouseEventId.MiddleDown:
                case MouseEventId.RightDown:
                case MouseEventId.LeftUp:
                case MouseEventId.MiddleUp:
                case MouseEventId.RightUp:
                    if (bounds && MouseInput.HandleEvent(data.EventId)) {
                        DoCallback(data.Callback, args);
                    }
                    break;
            }
        }
    }

    internal void HandleCallbacks(Sprite sprite) {
        var pos = MouseInput.GetMousePosition();
        var delta = Math.Max(Math.Min(MouseInput.GetScrollDelta(), 1), -1);
        var inBounds = sprite.IsInBounds(pos);
        var shift = KeyboardInput.IsKeyDown(Keys.LeftShift) || KeyboardInput.IsKeyDown(Keys.RightShift);
        var ctrl = KeyboardInput.IsKeyDown(Keys.LeftControl) || KeyboardInput.IsKeyDown(Keys.RightControl);
        var alt = KeyboardInput.IsKeyDown(Keys.LeftAlt) || KeyboardInput.IsKeyDown(Keys.RightAlt);

        var args = new MouseEventArgs(sprite, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetScrollDelta(), 1), -1), shift, ctrl, alt);
        
        foreach (var data in _callbacks) {
            var bounds = data.IgnoreBounds || inBounds;
            switch (data.EventId) {
                case MouseEventId.MouseMove when pos != _prevMousePosition:
                    _prevMousePosition = pos;
                    DoCallback(data.Callback, args);
                    break;
                case MouseEventId.MouseOver when bounds && !_prevMouseInBounds:
                    _prevMouseInBounds = true;
                    DoCallback(data.Callback, args);
                    break;
                case MouseEventId.Scroll when bounds:
                    if (delta == 0f) {
                        break;
                    }
                    
                    DoCallback(data.Callback, args);
                    break;
                case MouseEventId.LeftClick:
                case MouseEventId.MiddleClick:
                case MouseEventId.RightClick:
                case MouseEventId.LeftDown:
                case MouseEventId.MiddleDown:
                case MouseEventId.RightDown:
                case MouseEventId.LeftUp:
                case MouseEventId.MiddleUp:
                case MouseEventId.RightUp:
                    if (bounds && MouseInput.HandleEvent(data.EventId)) {
                        DoCallback(data.Callback, args);
                    }
                    break;
            }
        }
    }

    private static void DoCallback(Delegate action, MouseEventArgs args) {
        switch (action) {
            case Action callback:
                callback();
                break;
            case Action<MouseEventArgs> callback:
                callback(args);
                break;
            default:
                throw new InvalidCastException("Callback has invalid signature");
        }
    }
    
    internal void AddEvent(MouseEventData eventData, bool global) {
        if (eventData.EventId is MouseEventId.MouseOver or MouseEventId.MouseOut && (global || eventData.IgnoreBounds)) {
            Console.WriteLine("MouseOver/MouseOut not supported for overriding default global/ignoreBounds values");
            return;
        }
        
        switch (eventData.Callback) {
            case Action:
            case Action<MouseEventArgs>:
                _pending.Enqueue(new EventData(eventData, global, true));
                break;
            default:
                Console.WriteLine($"Unable to add callback, invalid signature, must be 'Callback()' or 'Callback(MouseEventArgs)' : {eventData.Callback.GetType().Name}");
                throw new Exception();
                break;
                    
        }
    }
    
    internal void RemoveEvent(MouseEventData eventData, bool global) {
        if (eventData.EventId is MouseEventId.MouseOver or MouseEventId.MouseOut && (global || eventData.IgnoreBounds)) {
            Console.WriteLine("MouseOver/MouseOut not supported for overriding default global/ignoreBounds values");
            return;
        }
        
        switch (eventData.Callback) {
            case Action:
            case Action<MouseEventArgs>:
                _pending.Enqueue(new EventData(eventData, global, false));
                break;
            default:
                Console.WriteLine("Unable to remove callback, invalid signature, must be 'Callback()' or 'Callback(MouseEventArgs)'");
                break;
                    
        }
    }

    private record EventData(MouseEventData Data, bool Global, bool Add);
}