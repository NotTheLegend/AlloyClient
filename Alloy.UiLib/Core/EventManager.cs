using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Input;
using Microsoft.Extensions.Logging;

namespace Alloy.UiLib.Core;

public abstract class EventManager {

    private static readonly ILogger Logger = UiRender.LogFactory.CreateLogger(nameof(EventManager));

    private static readonly HashSet<EventType<Event>> BroadcastEvents = [Event.EnterFrame];
    
    internal EventManager() { }

    private enum QueueState {
        Add,
        Remove,
    }

    private record BroadcastData(string Type, EventManager Manager, QueueState State);
    private record EventData(string Type, Listener Listener, QueueState State);
    private sealed record Listener(Delegate Callback, bool UseCapture);

    #region Broadcast

    private static readonly Queue<BroadcastData> BroadcastQueue = [];

    private static readonly Dictionary<string, List<EventManager>> BroadcastMap = new();
    
    private static bool _isBroadcasting;

    #endregion
    
    
    private readonly Dictionary<string, Queue<EventData>> _pending = [];

    private readonly Queue<EventType<MouseEvent>> _pendingClicks = [];

    

    private readonly Dictionary<string, List<Listener>> _eventMap = [];

    private static readonly Queue<Action> CompletedTasks = [];

    private readonly Stack<string> _currentEventState = [];
    
    
    private static TaskState GetStatus(Task task) {
        if (task.IsFaulted) return TaskState.Faulted;
        if (task.IsCanceled) return TaskState.Canceled;
        return TaskState.Completed;
    }

    private void QueueTaskFinish(Action callback) {
        CompletedTasks.Enqueue(callback);
    }
    
    public void AddEventListener(Task task, Action callback) {
        task.ContinueWith(_ => {
            if (task.IsFaulted) {
                Logger.Log(LogLevel.Error, task.Exception, "Task Failed");
            }
            QueueTaskFinish(callback);
        });
    }
    
    public void AddEventListener(Task task, Action<TaskState> callback) {
        task.ContinueWith(t => {
            if (task.IsFaulted) {
                Logger.Log(LogLevel.Error, task.Exception, "Task Failed");
            }
            QueueTaskFinish(() => callback(GetStatus(t)));
        });
    }
    
    public void AddEventListener<T>(Task<T> task, Action<T> callback) {
        task.ContinueWith(t => {
            if (task.IsFaulted) {
                Logger.Log(LogLevel.Error, task.Exception, "Task Failed");
            }
            QueueTaskFinish(() => callback(t.Result));
        });
    }
    
    public void AddEventListener<T>(Task<T> task, Action<T, TaskState> callback) {
        task.ContinueWith(t => {
            if (task.IsFaulted) {
                Logger.Log(LogLevel.Error, task.Exception, "Task Failed");
            }
            QueueTaskFinish(() => callback(t.Result, GetStatus(t)));
        });
    }
    
    public void AddEventListener<T>(EventType<T> type, Action callback, bool capture = false) where T : Event => AddEventListener(type, new Listener(callback, capture));
    
    public void AddEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) where T : Event => AddEventListener(type, new Listener(callback, capture));

    public void AddEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) where T : Event => AddEventListener(type, new Listener(callback, capture));

    private void AddEventListener<T>(EventType<T> type, Listener listener) where T : Event {
        if (_eventMap.TryGetValue(type, out var listeners) && listeners.Contains(listener)) return;
        HandleListener(new EventData(type, listener, QueueState.Add));
    }
    
    public void RemoveEventListener<T>(EventType<T> type, Action callback, bool capture = false) where T : Event => RemoveEventListener(type, new Listener(callback, capture));
    
    public void RemoveEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) where T : Event => RemoveEventListener(type, new Listener(callback, capture));
    
    public void RemoveEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) where T : Event => RemoveEventListener(type, new Listener(callback, capture));

    private void RemoveEventListener<T>(EventType<T> type, Listener listener) where T : Event {
        if (!_eventMap.TryGetValue(type, out var listeners)) return;
        if (!listeners.Contains(listener)) return;
        HandleListener(new EventData(type, listener, QueueState.Remove));
    }

    private void HandleListener(EventData pending) {
        if (_currentEventState.Contains(pending.Type)) {
            if (_pending.TryGetValue(pending.Type, out var queue)) {
                queue.Enqueue(pending);
            } else {
                _pending[pending.Type] = [];
                _pending[pending.Type].Enqueue(pending);
            }
            return;
        }
        
        if (IsBroadcast(pending.Type)) {
            HandleBroadcastListener(new BroadcastData(pending.Type, this, pending.State));
        }

        var hasType = _eventMap.TryGetValue(pending.Type, out var listeners);
        if (pending.State == QueueState.Add && !hasType)
            _eventMap[pending.Type] = listeners = [];
            
        switch (pending.State) {
            case QueueState.Add:
                listeners!.Add(pending.Listener);
                break;
            case QueueState.Remove when hasType:
                listeners.Remove(pending.Listener);
                break;
        }
    }

    private static void HandleBroadcastListener(BroadcastData pending) {
        if (_isBroadcasting) {
            BroadcastQueue.Enqueue(pending);
            return;
        }
        
        var hasType = BroadcastMap.TryGetValue(pending.Type, out var managers);
        if (pending.State == QueueState.Add && !hasType) BroadcastMap[pending.Type] = managers = [];
        
        switch (pending.State) {
            case QueueState.Add:
                managers!.Add(pending.Manager);
                break;
            case QueueState.Remove when hasType:
                managers.Remove(pending.Manager);
                break;
        }
    }

    private static bool IsBroadcast(EventType<Event> type) {
        return BroadcastEvents.Contains(type);
    }

    private void HandlePending(string type) {
        if (!_pending.TryGetValue(type, out var queue))
            return;
        
        while (queue.TryDequeue(out var pending)) {
            HandleListener(pending);
        }
    }

    private static void HandlePendingBroadcast() {
        while (BroadcastQueue.TryDequeue(out var pending)) {
            HandleBroadcastListener(pending);
        }
    }
    
    internal static void HandleFinishedTasks() {
        while (CompletedTasks.TryDequeue(out var callback)) {
            callback();
        }
    }

    internal void DispatchMouseEvents() {
        var keyboard = (this as Sprite)!.Stage.Keyboard;
        while(MouseInput.Events.TryDequeue(out var type)){
            DispatchEvent(new MouseEvent(type, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetVerticalScrollDelta(), 1), -1), keyboard.IsShiftDown(), keyboard.IsCtrlDown(), keyboard.IsAltDown()));
            CheckClicks(type);
        }

        while (_pendingClicks.TryDequeue(out var type)) {
            DispatchEvent(new MouseEvent(type, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetVerticalScrollDelta(), 1), -1), keyboard.IsShiftDown(), keyboard.IsCtrlDown(), keyboard.IsAltDown()));
        }
    }
    
    private static Sprite _leftTarget;
    private static Sprite _middleTarget;
    private static Sprite _rightTarget;

    private void CheckClicks(EventType<MouseEvent> type) {
        var sprite = this as Sprite;
        switch (type.Id) {
            case MouseEvent.LeftDownKey:
                _leftTarget = sprite;
                break;
            case MouseEvent.LeftUpKey:
                if (_leftTarget == sprite) {
                    if (TextInput.ActiveInput != null && sprite != TextInput.ActiveInput) {
                        TextInput.ActiveInput.UnFocus();
                    }
                    
                    _pendingClicks.Enqueue(MouseEvent.LeftClick);
                }
                _leftTarget = null;
                break;
            case MouseEvent.MiddleDownKey:
                _middleTarget = sprite;
                break;
            case MouseEvent.MiddleUpKey:
                if (_middleTarget == sprite)
                    _pendingClicks.Enqueue(MouseEvent.MiddleClick);
                _middleTarget = null;
                break;
            case MouseEvent.RightDownKey:
                _rightTarget = sprite;
                break;
            case MouseEvent.RightUpKey:
                if (_rightTarget == sprite)
                    _pendingClicks.Enqueue(MouseEvent.RightClick);
                _rightTarget = null;
                break;
        }
    }

    internal static void BroadcastEvent(Event @event) {
        if (!BroadcastMap.TryGetValue(@event.Type, out var sprites))
            return;

        _isBroadcasting = true;

        foreach (var sprite in sprites) {
            sprite.DispatchEvent(@event);
        }

        _isBroadcasting = false;
        HandlePendingBroadcast();
    }

    public bool DispatchEvent(Event @event) {
        if (string.IsNullOrWhiteSpace(@event.Type)) throw new Exception("Event Type must not be null, empty, or whitespace");
        
        var bubble = @event.Bubbles;

        if (!bubble && !_eventMap.ContainsKey(@event.Type))
            return false;

        var prev = @event.Target;
        @event.SetTarget(this as Sprite);

        bool stop;
        if (bubble) {
            stop = BubbleEvent(@event);
        } else {
           stop = InvokeEvent(@event, EventPhase.Target);
        }
        
        @event.SetTarget(prev);
        return stop;
    }
    
    private bool InvokeMouseEvent(MouseEvent mouseEvent, List<Listener> listeners, EventPhase phase) {
        var sprite = this as Sprite;

        if (!sprite!.MouseEnabled)
            return false;
        
        mouseEvent.SetCurrentTarget(sprite);
        mouseEvent.Phase = phase;
        
        var inBounds = sprite!.IsInBounds(mouseEvent.Coords);
        var button = MouseEvent.IsButtonType(mouseEvent.Type);

        if (button && !inBounds)
            return false;
        
        _currentEventState.Push(mouseEvent.Type);
        
        var isCapture = mouseEvent.Phase == EventPhase.Capture;
        foreach (var listener in listeners) {
            if (isCapture && !listener.UseCapture)
                continue;

            if (listener.Callback is Action callback)
                callback();
            else
                listener.Callback.DynamicInvoke(mouseEvent);
            
            if (mouseEvent.ImmediateStop)
                break;
        }
        
        HandlePending(_currentEventState.Pop());
        
        return mouseEvent.Stop || mouseEvent.ImmediateStop;
    }

    private bool InvokeEvent(Event @event, EventPhase phase) {
        var has = _eventMap.TryGetValue(@event.Type, out var listeners);
        if (!has || listeners.Count < 1)
            return false;

        if (@event is MouseEvent mouseEvent)
            return InvokeMouseEvent(mouseEvent, listeners, phase);
        
        @event.SetCurrentTarget(this as Sprite);
        @event.Phase = phase;
        
        _currentEventState.Push(@event.Type);

        var isCapture = phase == EventPhase.Capture;
        foreach (var listener in listeners) {
            if (isCapture && !listener.UseCapture)
                continue;

            if (listener.Callback is Action callback)
                callback();
            else
                listener.Callback.DynamicInvoke(@event);
            
            if (@event.ImmediateStop)
                break;
        }
        HandlePending(_currentEventState.Pop());
        
        return @event.Stop || @event.ImmediateStop;
    }
    
    private bool BubbleEvent(Event @event) {
        var chain = new List<EventManager>();
        
        var obj = this as DisplayContainer;

        while ((obj = obj!.Parent) != null) {
            chain.Add(obj);
        }

        //capture phase
        for (var i = chain.Count - 1; i >= 0; i--) {
            if (chain[i].InvokeEvent(@event, EventPhase.Capture))
                return true;
        }

        // target phase
        if (InvokeEvent(@event, EventPhase.Target))
            return true;

        // bubble phase
        for (var i = 0; i < chain.Count; i++) {
            if (chain[i].InvokeEvent(@event, EventPhase.Bubble))
                return true;
        }

        return false;
    }
}