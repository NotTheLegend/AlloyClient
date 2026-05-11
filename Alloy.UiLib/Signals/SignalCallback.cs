using System;
using System.Reflection;

namespace Alloy.UiLib.Signals;

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