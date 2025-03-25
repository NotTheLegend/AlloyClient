using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Input;

namespace MonoClient.UiLib.Core;

public abstract class EventManager {

    private enum QueueState {
        Add,
        Remove,
        Clear,
        ClearAll
    }
    
    private record EventData(string Type, Delegate Callback, bool Capture, QueueState State);
    
    private readonly Queue<EventData> _pending = new();

    private readonly Dictionary<string, List<Delegate>> _eventListeners = [];
    private readonly Dictionary<string, List<Delegate>> _captureEventListeners = [];

    private Dictionary<string, List<Delegate>> GetListeners(bool capture) => capture ? _captureEventListeners : _eventListeners;

    public void AddEventListener(string type, Delegate callback, bool capture = false) {
        ValidateAgainstBuiltIn(type, callback);
        
        var listeners = GetListeners(capture);
        
        if (listeners.TryGetValue(type, out var list) && list.Contains(callback)) return;
        
        _pending.Enqueue(new EventData(type, callback, capture, QueueState.Add));
    }
    
    public void RemoveEventListener(string type, Delegate callback, bool capture = false) {
        var listeners = GetListeners(capture);
        
        if (!listeners.TryGetValue(type, out var list)) return;
        if (!list.Contains(callback)) return;
        
        _pending.Enqueue(new EventData(type, callback, capture, QueueState.Remove));
    }

    public void RemoveEventListeners(string type = null, bool capture = false) {
        var listeners = GetListeners(capture);
        
        if (type == null) {
            _pending.Enqueue(new EventData("", null, capture, QueueState.ClearAll));
            return;
        }

        if (listeners.ContainsKey(type)) {
            _pending.Enqueue(new EventData(type, null, capture, QueueState.Clear));
        }
    }

    private static void ValidateAgainstBuiltIn(string type, Delegate callback) {
        switch (callback) {
            case Action:
            case Action<Event>:
            case Action<KeyboardEvent> when KeyboardEvent.ValidateType(type):
            case Action<MouseEvent> when MouseEvent.ValidateType(type):
                return;
            default:
                throw new InvalidCallbackException("Invalid signature for defined callback");
            
        }
    }
    
    internal void UpdateNormalListeners() {
        List<Delegate> list;
        Dictionary<string, List<Delegate>> listeners;
        while (_pending.TryDequeue(out var pending)) {
            listeners = GetListeners(pending.Capture);
            switch (pending.State) {
                case QueueState.Add:
                    if (listeners.TryGetValue(pending.Type, out list)) {
                        list.Add(pending.Callback);
                        break;
                    }
                    listeners[pending.Type] = [pending.Callback];
                    break;
                case QueueState.Remove:
                    if (listeners.TryGetValue(pending.Type, out list)) {
                        list.Remove(pending.Callback);
                        if (list.Count == 0) {
                            listeners.Remove(pending.Type);
                        }
                    }
                    break;
                case QueueState.Clear:
                    listeners.Remove(pending.Type);
                    break;
                case QueueState.ClearAll:
                    listeners.Clear();
                    break;
            }
        }
    }

    internal void DispatchMouseEvents() {
        foreach (var type in MouseInput.Events) {
            DispatchEvent(new MouseEvent(type, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetScrollDelta(), 1), -1), KeyboardInput.IsShiftDown(), KeyboardInput.IsCtrlDown(), KeyboardInput.IsAltDown()));
        }
    }

    public void DispatchEvent(Event @event) {
        var bubble = @event.Bubbles;
        
        if (!bubble && !_eventListeners.ContainsKey(@event.Type))
            return;

        var prev = @event.Target;
        @event.SetTarget(this as Sprite);

        if (bubble) {
            BubbleEvent(@event);
        } else {
            InvokeEvent(@event, false);
        }
        @event.SetTarget(prev);
    }

    private bool InvokeMouseEvent(MouseEvent mouseEvent, List<Delegate> listeners) {
        var sprite = this as Sprite;

        if (!sprite!.MouseEnabled)
            return false;
        
        mouseEvent.SetCurrentTarget(sprite);
        
        var inBounds = sprite!.IsInBounds(mouseEvent.Coords);
        var button = MouseEvent.IsButtonType(mouseEvent.Type);

        if (button && !inBounds)
            return false;
        
        var len = listeners.Count;
        for (var i = 0; i < len; i++) {
            var cb = listeners[i];
            
            switch (cb) {
                case Action callback: callback();
                    break;
                default: cb.DynamicInvoke(mouseEvent);
                    break;
            }

            if (mouseEvent.ImmediateStop)
                return true;
        }
        
        return mouseEvent.Stop;
    }

    private bool InvokeEvent(Event @event, bool capture) {
        var has = GetListeners(capture).TryGetValue(@event.Type, out var listeners);
        if (!has || listeners.Count < 1)
            return false;

        if (MouseEvent.ValidateType(@event.Type))
            return InvokeMouseEvent(@event as MouseEvent, listeners);
        
        @event.SetCurrentTarget(this as Sprite);
        
        var len = listeners.Count;
        for (var i = 0; i < len; i++) {
            var cb = listeners[i];
            
            switch (cb) {
                case Action callback: callback();
                    break;
                default: cb.DynamicInvoke(@event);
                    break;
            }

            if (@event.ImmediateStop)
                return true;
        }
        
        return @event.Stop;
    }
    
    private void BubbleEvent(Event @event) {
        var chain = new List<EventManager>();
        
        var obj = this as Sprite;

        while ((obj = obj!.Parent) != null) {
            chain.Add(obj);
        }

        //capture phase
        for (var i = chain.Count - 1; i >= 0; i--) {
            if (chain[i].InvokeEvent(@event, true))
                return;
        }

        // target phase
        if (InvokeEvent(@event, false))
            return;

        // bubble phase
        for (var i = 0; i < chain.Count; i++) {
            if (chain[i].InvokeEvent(@event, false))
                return;
        }
    }

}