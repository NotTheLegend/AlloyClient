using System;
using System.Collections.Generic;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.UiLib.Core.Events;

// holds weak references of delegates
// might be useful later if callbacks end up causing mem leaks
/*public sealed class NewWeakReference<T> : IEquatable<NewWeakReference<T>> where T : Delegate {

    private readonly WeakReference<T> _reference;

    private readonly int _callbackHash;

    public NewWeakReference(T callback) {
        _reference = new WeakReference<T>(callback);
        _callbackHash = callback.GetHashCode();
    }

    public bool GetCallback(out T callback) {
        _reference.TryGetTarget(out callback);
        return callback != null;
    }

    public bool Equals(NewWeakReference<T> other) {
        if (other is null) return false;
        return _callbackHash == other._callbackHash;
    }

    public override bool Equals(object obj) {
        return obj is NewWeakReference<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return _callbackHash;
    }
}*/

internal class EventHandler {

    private readonly Queue<IEventData> _dispatch = new(25);
    
    private readonly Queue<(InternalEventInfo, bool)> _pending = new();
    
    private readonly CallbackDictionary _callbacks = [];

    internal void Dispatch(IEventData data) => _dispatch.Enqueue(data);

    internal void Handle(Sprite sprite) {
        while (_pending.TryDequeue(out var pending)) {
            if (pending.Item2) {
                _callbacks.Add(pending.Item1);
            } else {
                _callbacks.Remove(pending.Item1);
            }
        }

        while (_dispatch.TryDequeue(out var data)) {
            if (!_callbacks.TryGetValue(data.Id, out var list)) continue;
            if (list.Count == 0) continue;

            var len = list.Count;
            for (var i = 0; i < len; i++) {
                MonoClient.UiLib.Core.Events.Types.Event.DoCallback(list[i].Callback, data, sprite);
            }
        }
    }
    
    internal void AddEvent(InternalEventInfo eventInfo) {
        if (MonoClient.UiLib.Core.Events.Types.Event.ValidateCallback(eventInfo)) 
            _pending.Enqueue((eventInfo, true));
        else 
            Console.WriteLine($"Unable to add callback, invalid signature. Must be 'Callback()', 'Callback(EventData)', 'Callback({eventInfo.Event})EventData'");
    }
    
    internal void RemoveEvent(InternalEventInfo eventData) {
        _pending.Enqueue((eventData, false));
    }
}

internal class CallbackDictionary : Dictionary<EventId, List<InternalEventInfo>> {

    public void Add(InternalEventInfo info) {
        if (!ContainsKey(info.Event)) {
            base[info.Event] = [];
        }
        
        base[info.Event].Add(info);
    }
    
    public void Remove(InternalEventInfo info) {
        if (!ContainsKey(info.Event)) {
            return;
        }
        
        base[info.Event].Remove(info);
    }
    
}