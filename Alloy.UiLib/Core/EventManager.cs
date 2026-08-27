using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Alloy.UiLib.Core;

public abstract class EventManager {

    private readonly static ILogger Logger = UiRender.LogFactory.CreateLogger(nameof(EventManager));

    private readonly static HashSet<EventType<Event>> BroadcastEvents = [Event.EnterFrame];

    internal EventManager() {
    }

    private sealed class Listener(Delegate callback, Action<Event> invoke, bool useCapture) {

        public readonly Delegate Callback = callback;

        public readonly Action<Event> Invoke = invoke;

        public readonly bool UseCapture = useCapture;

        public bool Matches(Delegate callback, bool useCapture) {
            return Callback == callback && UseCapture == useCapture;
        }
    }

    private enum QueueState {
        Add,
        Remove,
    }

    private record BroadcastData(string Type, EventManager Manager, QueueState State);

    private record EventData(string Type, Listener Listener, QueueState State);

    #region Broadcast

    private readonly static Queue<BroadcastData> BroadcastQueue = [];

    private readonly static Dictionary<string, List<EventManager>> BroadcastMap = new();

    private static bool _isBroadcasting;

    #endregion

    private readonly Dictionary<string, Queue<EventData>> _pending = [];

    private readonly Dictionary<string, List<Listener>> _eventMap = [];

    private readonly static Queue<Action> CompletedTasks = [];

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

    public void AddEventListener<T>(EventType<T> type, Action callback, bool capture = false) where T : Event {
        AddEventListener(type, callback, _ => callback(), capture);
    }

    public void AddEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) where T : Event {
        AddEventListener(type, callback, callback, capture);
    }

    public void AddEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) where T : Event {
        AddEventListener(type, callback, @event => callback((T)@event), capture);
    }

    private void AddEventListener<T>(EventType<T> type, Delegate callback, Action<Event> invoke, bool capture) where T : Event {
        if (_eventMap.TryGetValue(type, out var listeners) && listeners.Exists(listener => listener.Matches(callback, capture))) {
            return;
        }

        var listener = new Listener(callback, invoke, capture);
        HandleListener(new EventData(type, listener, QueueState.Add));
    }

    public void RemoveEventListener<T>(EventType<T> type, Action callback, bool capture = false) where T : Event {
        RemoveEventListenerInternal(type, callback, capture);
    }

    public void RemoveEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) where T : Event {
        RemoveEventListenerInternal(type, callback, capture);
    }

    public void RemoveEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) where T : Event {
        RemoveEventListenerInternal(type, callback, capture);
    }

    private void RemoveEventListenerInternal<T>(EventType<T> type, Delegate callback, bool capture) where T : Event {
        if (!_eventMap.TryGetValue(type, out var listeners)) {
            return;
        }

        var listener = listeners.Find(item => item.Matches(callback, capture));
        if (listener is null) {
            return;
        }

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
        if (pending.State == QueueState.Add && !hasType) {
            _eventMap[pending.Type] = listeners = [];
        }

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
        if (!_pending.TryGetValue(type, out var queue)) {
            return;
        }

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

    internal static void BroadcastEvent(Event @event) {
        if (!BroadcastMap.TryGetValue(@event.Type, out var sprites)) {
            return;
        }

        _isBroadcasting = true;

        foreach (var sprite in sprites) {
            sprite.DispatchEvent(@event);
        }

        _isBroadcasting = false;
        HandlePendingBroadcast();
    }

    public bool DispatchEvent(Event @event) {
        if (string.IsNullOrWhiteSpace(@event.Type)) {
            throw new Exception("Event Type must not be null, empty, or whitespace");
        }

        var bubble = @event.Bubbles;

        if (!bubble && !_eventMap.ContainsKey(@event.Type)) {
            return false;
        }

        var prev = @event.Target;
        @event.SetTarget(this as Sprite);

        var stop = bubble ? BubbleEvent(@event) : InvokeEvent(@event, EventPhase.Target);

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

        if (button && !mouseEvent.Captured && !inBounds)
            return false;

        _currentEventState.Push(mouseEvent.Type);

        var isCapture = mouseEvent.Phase == EventPhase.Capture;
        foreach (var listener in listeners) {
            if (isCapture && !listener.UseCapture) {
                continue;
            }

            listener.Invoke(mouseEvent);

            if (mouseEvent.ImmediateStop) {
                break;
            }
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
            if (isCapture && !listener.UseCapture) {
                continue;
            }

            listener.Invoke(@event);

            if (@event.ImmediateStop) {
                break;
            }
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

        // Capture phase
        for (var i = chain.Count - 1; i >= 0; i--) {
            if (chain[i].InvokeEvent(@event, EventPhase.Capture)) {
                return true;
            }
        }

        // Target phase
        if (InvokeEvent(@event, EventPhase.Target)) {
            return true;
        }

        // Bubble phase
        foreach (var manager in chain) {
            if (manager.InvokeEvent(@event, EventPhase.Bubble)) {
                return true;
            }
        }

        return false;
    }
}