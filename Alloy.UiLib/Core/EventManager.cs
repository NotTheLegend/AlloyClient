using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alloy.UiLib.Utils;
using Microsoft.Extensions.Logging;

namespace Alloy.UiLib.Core;

public class EventManager {

    private static readonly ILogger Logger = UiRender.LogFactory.CreateLogger(nameof(EventManager));
    
    private record Listener(Delegate Callback, bool UseCapture);
    
    private readonly Dictionary<string, CachedList<Listener>> _eventMap = [];
    
    public virtual void AddEventListener<T>(EventType<T> type, Action callback, bool capture = false) where T : Event => HandleListener(type, new Listener(callback, capture), true);
    public virtual void AddEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) where T : Event => HandleListener(type, new Listener(callback, capture), true);
    public virtual void AddEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) where T : Event => HandleListener(type, new Listener(callback, capture), true);
    
    public virtual void RemoveEventListener<T>(EventType<T> type, Action callback, bool capture = false) where T : Event => HandleListener(type, new Listener(callback, capture), false);
    public virtual void RemoveEventListener<T>(EventType<T> type, Action<Event> callback, bool capture = false) where T : Event => HandleListener(type, new Listener(callback, capture), false);
    public virtual void RemoveEventListener<T>(EventType<T> type, Action<T> callback, bool capture = false) where T : Event => HandleListener(type, new Listener(callback, capture), false);

    private void HandleListener(string type, Listener listener, bool add) {
        if (!_eventMap.TryGetValue(type, out var listeners)) {
            _eventMap[type] = listeners = [];
        }
        
        if (!add) {
            listeners.Remove(listener);
        } else if (!listeners.Contains(listener)) {
            listeners.Add(listener);
        }
    }

    public virtual void DispatchEvent(Event @event) {
        if (@event is null || string.IsNullOrWhiteSpace(@event.Type)) {
            throw new Exception("Event or Event.Type must not be null, empty, or whitespace");
        }

        DispatchEventInternal(@event);
    }

    private protected virtual void DispatchEventInternal(Event @event) {
        if (!_eventMap.TryGetValue(@event.Type, out var listeners)) {
            return;
        }

        var isCapture = @event.Phase == EventPhase.Capture;

        foreach (var listener in listeners) {
            if (isCapture != listener.UseCapture)
                continue;
            
            if (listener.Callback is Action callback) {
                callback();
            } else {
                listener.Callback.DynamicInvoke(@event);
            }
            
            if (@event.ImmediateStop)
                break;
        }
    }

    // TODO: change/redo or something with task event listeners, this aint it
    #region TaskEvents
    
    private static readonly ConcurrentQueue<Action> CompletedTasks = [];
    
    internal static void HandleFinishedTasks() {
        while (CompletedTasks.TryDequeue(out var callback)) {
            callback();
        }
    }
    
    private static TaskState GetStatus(Task task) {
        if (task.IsFaulted) return TaskState.Faulted;
        if (task.IsCanceled) return TaskState.Canceled;
        return TaskState.Completed;
    }

    private void QueueTaskFinish(Action callback) => CompletedTasks.Enqueue(callback);

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
    
    #endregion
}