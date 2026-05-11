using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Alloy.UiLib.Core;

public enum EventPhase {
    Capture,
    Target,
    Bubble
}

public record struct EventType<T>(string Id) where T : Event {
    public static implicit operator EventType<T>(string id) {
        return new EventType<T>(id);
    }
    
    public static implicit operator string(EventType<T> type) {
        return type.Id;
    }
}

public class Event(EventType<Event> type, bool bubbles = false) {
    public readonly string Type = type.Id;
    public readonly bool Bubbles = bubbles;

    public Sprite Target { get; private set; }
    
    public Sprite CurrentTarget { get; private set; }
    
    public EventPhase Phase { get; internal set; }

    internal bool Stop;
    
    internal bool ImmediateStop;

    internal void SetTarget(Sprite target) => Target = target;
    internal void SetCurrentTarget(Sprite target) => CurrentTarget = target;

    public void StopPropagation() => Stop = true;
    
    public void StopImmediatePropagation() => ImmediateStop = true;
    
    public static readonly EventType<Event> AddedToStage = "addedToStage";
    public static readonly EventType<Event> RemovedFromStage = "removedFromStage";
    public static readonly EventType<Event> Added = "added";
    public static readonly EventType<Event> Removed = "removed";
    public static readonly EventType<Event> EnterFrame = "enterFrame";
}

/// <summary>
/// Keyboard events are *ONLY* dispatched on stage layer, if listeners are put on any other sprite they will not trigger!
/// </summary>
public class KeyboardEvent(EventType<KeyboardEvent> type, Key key, Scancode code, bool ctrl, bool shift, bool alt) : Event(type.Id, true) {
    public readonly Key Key = key;
    public readonly Scancode Code = code;
    public readonly bool Ctrl = ctrl;
    public readonly bool Shift = shift;
    public readonly bool Alt = alt;
    
    public static readonly EventType<KeyboardEvent> KeyDown = "keyDown";
    public static readonly EventType<KeyboardEvent> KeyUp = "keyUp";
}

public class MouseEvent(EventType<MouseEvent> type, Vector2i coords = new(), float delta = 0f, bool shiftKey = false, bool ctrlKey = false, bool altKey = false) : Event(type.Id, true) {
    public readonly Vector2i Coords = coords;
    public readonly float Delta= delta;
    public readonly bool ShiftKey = shiftKey;
    public readonly bool CtrlKey = ctrlKey;
    public readonly bool AltKey = altKey;
    
    internal const string LeftClickKey = "leftClick";
    internal const string MiddleClickKey  = "middleClick";
    internal const string RightClickKey  = "rightClick";
    internal const string MouseOverKey  = "mouseOver";
    internal const string MouseOutKey  = "mouseOut";
    internal const string LeftDownKey = "leftDown";
    internal const string MiddleDownKey  = "middleDown";
    internal const string RightDownKey  = "rightDown";
    internal const string LeftUpKey = "leftUp";
    internal const string MiddleUpKey  = "middleUp";
    internal const string RightUpKey  = "rightUp";
    internal const string MouseMoveKey  = "mouseMove";
    internal const string ScrollVerticalKey  = "scrollVertical";
    internal const string ScrollHorizontalKey  = "scrollHorizontal";

    public static readonly EventType<MouseEvent> LeftClick = LeftClickKey;
    public static readonly EventType<MouseEvent> MiddleClick = MiddleClickKey;
    public static readonly EventType<MouseEvent> RightClick = RightClickKey;
    public static readonly EventType<MouseEvent> MouseOver = MouseOverKey;
    public static readonly EventType<MouseEvent> MouseOut = MouseOutKey;
    public static readonly EventType<MouseEvent> LeftDown = LeftDownKey;
    public static readonly EventType<MouseEvent> MiddleDown = MiddleDownKey;
    public static readonly EventType<MouseEvent> RightDown = RightDownKey;
    public static readonly EventType<MouseEvent> LeftUp = LeftUpKey;
    public static readonly EventType<MouseEvent> MiddleUp = MiddleUpKey;
    public static readonly EventType<MouseEvent> RightUp = RightUpKey;
    public static readonly EventType<MouseEvent> MouseMove = MouseMoveKey;
    public static readonly EventType<MouseEvent> ScrollVertical = ScrollVerticalKey;
    public static readonly EventType<MouseEvent> ScrollHorizontal = ScrollHorizontalKey;

    private static readonly HashSet<EventType<MouseEvent>> ButtonTypes = [LeftClick, MiddleClick, RightClick, LeftDown, MiddleDown, RightUp, LeftUp, MiddleUp, RightUp, ScrollVertical, ScrollHorizontal];
    
    internal static bool IsButtonType(EventType<MouseEvent> type) {
        return ButtonTypes.Contains(type);
    }
}

/// <summary>
/// Resize events are *ONLY* dispatched on stage layer, if listeners are put on any other sprite they will not trigger!
/// </summary>
public class ResizeEvent(EventType<ResizeEvent> type, int width, int height) : Event(type.Id) {
    public readonly int Width = width;
    public readonly int Height = height;
    
    public static readonly EventType<ResizeEvent> Resize = "resize";
}