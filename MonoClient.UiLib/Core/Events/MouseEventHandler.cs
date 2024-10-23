using System;
using System.Collections.Generic;
using Common.Vector;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Input;

namespace MonoClient.UiLib.Core.Events;

internal class MouseEventHandler {
    private readonly Queue<(MouseEventData, bool)> _pending = new();
    private readonly List<MouseEventData> _callbacks = [];
    
    private bool _prevMouseInBounds;
    private IntVector2 _prevMousePosition;

    internal void Handle(Sprite sprite, ref MouseEventId consumed) {
        while (_pending.TryDequeue(out var pending)) {
            if (pending.Item2) {
                _callbacks.Add(pending.Item1);
            } else {
                _callbacks.Remove(pending.Item1);
            }
        }
        
        var pos = MouseInput.GetMousePosition();
        var delta = Math.Max(Math.Min(MouseInput.GetScrollDelta(), 1), -1);
        var inBounds = sprite.IsInBounds(pos);
        var flags = (MouseEventId) 0;
        foreach (var data in _callbacks) {
            var clear = !data.Consume || (consumed & data.EventId) == 0;
            var bounds = data.Global || inBounds;
            switch (data.EventId) {
                case MouseEventId.MouseMove when pos != _prevMousePosition:
                    _prevMousePosition = pos;
                    DoCallback(data.Callback, sprite);
                    break;
                case MouseEventId.MouseOver when clear && bounds && !_prevMouseInBounds:
                    _prevMouseInBounds = true;
                    DoCallback(data.Callback, sprite);
                    if (data.Consume) {
                        flags |= data.EventId;
                    }
                    break;
                case MouseEventId.MouseOut when !bounds && _prevMouseInBounds:
                case MouseEventId.MouseOut when (consumed & MouseEventId.MouseOver) != 0 && _prevMouseInBounds:
                    _prevMouseInBounds = false;
                    DoCallback(data.Callback, sprite);
                    break;
                case MouseEventId.Scroll when clear && bounds:
                    if (delta == 0f) {
                        break;
                    }
                    
                    DoCallback(data.Callback, sprite);
                    if (data.Consume) {
                        flags |= data.EventId;
                    }
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
                    if (clear && bounds && MouseInput.HandleEvent(data.EventId)) {
                        DoCallback(data.Callback, sprite);
                        if (data.Consume) {
                            flags |= data.EventId;
                        }
                    }
                    break;
            }
        }
        
        if(MouseInput.HandleEvent(MouseEventId.LeftClick) && TextInput.UnFocusOnClick)
            TextInput.ActiveInput?.UnFocus();

        consumed |= flags;
    }

    private static void DoCallback(Delegate action, Sprite sprite) {
        switch (action) {
            case Action callback:
                callback();
                break;
            case Action<MouseEventArgs> callback:
                callback(new MouseEventArgs(sprite, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetScrollDelta(), 1), -1)));
                break;
            default:
                throw new InvalidCastException("Callback has invalid signature");
        }
    }
    
    internal void AddEvent(MouseEventData eventData) {
        switch (eventData.Callback) {
            case Action:
            case Action<MouseEventArgs>:
                _pending.Enqueue((eventData, true));
                break;
            default:
                Console.WriteLine("Unable to add callback, invalid signature, must be 'Callback()' or 'Callback(MouseEventArgs)'");
                break;
                    
        }
    }
    
    internal void RemoveEvent(MouseEventData eventData) {
        switch (eventData.Callback) {
            case Action:
            case Action<MouseEventArgs>:
                _pending.Enqueue((eventData, false));
                break;
            default:
                Console.WriteLine("Unable to remove callback, invalid signature, must be 'Callback()' or 'Callback(MouseEventArgs)'");
                break;
                    
        }
    }
}