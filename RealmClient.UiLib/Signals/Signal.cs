using System;
using System.Collections.Generic;

namespace RealmClient.UiLib.Signals;

public sealed class Signal {

    private readonly List<SignalCallback<Action>> _listeners = [];

    public void Add(Action callback) {
        _listeners.Add(new SignalCallback<Action>(callback));
    }
    
    public void Remove(Action callback) {
        _listeners.Remove(new SignalCallback<Action>(callback));
    }

    public void RemoveAll() {
        _listeners.Clear();
    }

    public void Dispatch() {
        for (var index = _listeners.Count - 1; index >= 0; index--) {
            if (!_listeners[index].GetCallback(out var callback)) {
                _listeners.RemoveAt(index);
                continue;
            }
            
            callback.Invoke();
        }
    }
}

public sealed class Signal<T> {

    private readonly List<SignalCallback<Action<T>>> _listeners = [];

    public void Add(Action<T> callback) {
        _listeners.Add(new SignalCallback<Action<T>>(callback));
    }

    public void Remove(Action<T> callback) {
        _listeners.Remove(new SignalCallback<Action<T>>(callback));
    }

    public void RemoveAll() {
        _listeners.Clear();
    }

    public void Dispatch(T data) {
        for (var index = _listeners.Count - 1; index >= 0; index--) {
            if (!_listeners[index].GetCallback(out var callback)) {
                _listeners.RemoveAt(index);
                continue;
            }

            callback.Invoke(data);
        }
    }
}