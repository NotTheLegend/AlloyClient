using System;

namespace MonoClient.UiLib.Signals;

public sealed class SingleSignal {

    private SignalCallback<Action> _listener;

    public void Set(Action callback) {
        _listener = new SignalCallback<Action>(callback);
    }
    
    public void Remove() {
        _listener = null;
    }

    public void Dispatch() {
        if (_listener == null || !_listener.GetCallback(out var callback)) {
            _listener = null;
            return;
        }
        
        callback.Invoke();
    }
}

public sealed class SingleSignal<T> {

    private SignalCallback<Action<T>> _listener;

    public void Set(Action<T> callback) {
        _listener = new SignalCallback<Action<T>>(callback);
    }

    public void Remove() {
        _listener = null;
    }

    public void Dispatch(T data) {
        if (_listener == null || !_listener.GetCallback(out var callback)) {
            _listener = null;
            return;
        }
        
        callback.Invoke(data);
    }
}

public sealed class SingleSignal<T1, T2> {

    private SignalCallback<Action<T1, T2>> _listener;

    public void Set(Action<T1, T2> callback) {
        _listener = new SignalCallback<Action<T1, T2>>(callback);
    }

    public void Remove() {
        _listener = null;
    }

    public void Dispatch(T1 data1, T2 data2) {
        if (_listener == null || !_listener.GetCallback(out var callback)) {
            _listener = null;
            return;
        }
        
        callback.Invoke(data1, data2);
    }
}