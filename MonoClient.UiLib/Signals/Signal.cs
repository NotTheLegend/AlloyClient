using System;
using System.Collections.Generic;
using System.Reflection;

namespace MonoClient.UiLib.Signals;

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


internal sealed class SignalCallback<T> : IEquatable<SignalCallback<T>> where T : Delegate {

    private readonly WeakReference<object> _reference;
    private readonly MethodInfo _callback;
    private readonly int _callbackHash;
    
    public SignalCallback(T callback) {
        _reference = new WeakReference<object>(callback.Target);
        _callback = callback.GetMethodInfo();
        _callbackHash = _callback.GetHashCode();
    }

    public bool GetCallback(out T callback) {
        var alive = _reference.TryGetTarget(out var obj);
        if (!alive) {
            callback = null;
            return false;
        }

        callback = Delegate.CreateDelegate(typeof(T), obj, _callback) as T;
        return true;
    }

    public bool Equals(SignalCallback<T> other) {
        if (other is null) return false;
        return _callbackHash == other._callbackHash;
    }

    public override bool Equals(object obj) {
        return obj is SignalCallback<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return _callbackHash;
    }
}