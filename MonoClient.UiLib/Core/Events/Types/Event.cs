using System;
using System.Collections.Generic;

namespace MonoClient.UiLib.Core.Events.Types;

public enum EventId {
    None = 0,
    Resize = 1,
}

internal static class Event {
    internal static void DoCallback(Delegate action, IEventData eventData, Sprite sprite) {
        switch (action) {
            case Action callback:
                callback();
                break;
            case Action<EventData> callback:
                callback(new EventData(sprite));
                break;
            case Action<ResizeEventData> callback when ValidateEventData(eventData, out ResizeEventData data) :
                callback(data);
                break;
            default:
                throw new InvalidCastException("Callback has invalid signature, This should not happen!");
        }
    }

    private static bool ValidateEventData<T>(IEventData data, out T type) {
        switch (data) {
            case T eventData:
                type = eventData;
                return true;
            default:
                type = default;
                return false;
            
        }
    }

    internal static bool ValidateCallback(InternalEventInfo info) {
        switch (info.Callback) {
            case Action:
            case Action<EventData>:
            case Action<ResizeEventData> when info.Event == EventId.Resize:
                return true;
            default:
                return false;
        }
    }
}

internal record InternalEventInfo(EventId Event, Delegate Callback);

internal record DispatchData(EventId Event, IEventData EventData);

public interface IEventData {
    internal EventId Id { get; }
}

internal readonly struct NoEventData(EventId id) : IEventData {
    EventId IEventData.Id => id;
}

public readonly struct EventData(Sprite sprite) : IEventData {
    EventId IEventData.Id => EventId.None;
    public readonly Sprite Sprite = sprite;
    
}

// !!! Naming scheme for event data is 'NameOfEnum' + EventData

public readonly struct ResizeEventData(int w, int h) : IEventData {
    EventId IEventData.Id => EventId.Resize;
    public readonly int Width = w;
    public readonly int Height = h;
    
}