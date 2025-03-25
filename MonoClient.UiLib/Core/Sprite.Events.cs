using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Input;
using Common.Vector;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using EventHandler = MonoClient.UiLib.Core.Events.EventHandler;

namespace MonoClient.UiLib.Core;

public partial class Sprite {
    
    private readonly TaskEventHandler _taskEventHandler = new();
    private readonly EventHandler _eventHandler = new();

    private void HandleGlobalEvents() {
        _eventHandler.Handle(this);
        _taskEventHandler.Handle();
    }
    
    public void DispatchEvent(EventId eventId) {
        _eventHandler.Dispatch(new NoEventData(eventId));
    }

    public void DispatchEvent(IEventData eventData) {
        _eventHandler.Dispatch(eventData);
    }

    public void AddEventListener(EventId eventId, Delegate callback) {
        _eventHandler.AddEvent(new InternalEventInfo(eventId, callback));
    }
    
    public void RemoveEventListener(EventId eventId, Delegate callback) {
        _eventHandler.RemoveEvent(new InternalEventInfo(eventId, callback));
    }
    
    /// <summary>
    /// Adds an task event listener to this sprite. The event is invoked on update if the method (task) completed execution.
    /// For listening to async methods, use Task.Run()
    /// </summary>
    /// <param name="type">Type of task event to add</param>
    /// <param name="task">Task to listen for</param>
    /// <param name="callback">Callback to invoke</param>
    public void AddEventListener(TaskEvent type, Task task, Action callback) {
        _taskEventHandler.AddEvent(type, task, callback);
    }

    public void AddEventListener<T>(TaskEvent type, Task<T> task, Action<T> callback) {
        _taskEventHandler.AddEvent(type, task, callback);
    }

    /// <summary>
    /// Removes an async task method event listener from this sprite
    /// </summary>
    /// <param name="type">Type of task event to remove</param>
    /// <param name="task">Task to remove</param>
    /// <param name="callback">Callback to remove</param>
    public void RemoveEventListener(TaskEvent type, Task task, Action callback) {
        _taskEventHandler.RemoveEvent(type, task, callback);
    }

    public void RemoveEventListener<T>(TaskEvent type, Task<T> task, Action<T> callback) {
        _taskEventHandler.RemoveEvent(type, task, callback);
    }
}