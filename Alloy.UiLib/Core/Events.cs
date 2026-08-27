using System;
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
    public static implicit operator EventType<T>(string id) =>
        string.IsNullOrWhiteSpace(id) ? throw new Exception() : new EventType<T>(id);

    public static implicit operator string(EventType<T> type) => type.Id;
}

public class Event(EventType<Event> type, bool bubbles = false, bool cancelable = false) {
    public readonly string Type = type.Id;
    public readonly bool Bubbles = bubbles;
    public readonly bool Cancelable = cancelable;

    public Sprite Target { get; private set; }

    public Sprite CurrentTarget { get; private set; }

    public EventPhase Phase { get; internal set; }

    internal bool Stop;

    internal bool ImmediateStop;

    private bool _defaultPrevented;

    internal void SetTarget(Sprite target) => Target = target;
    internal void SetCurrentTarget(Sprite target) => CurrentTarget = target;

    public void StopPropagation() => Stop = true;

    public void StopImmediatePropagation() => ImmediateStop = true;

    public void PreventDefault() {
        if (Cancelable) {
            _defaultPrevented = true;
        }
    }

    public bool IsDefaultPrevented() {
        return _defaultPrevented;
    }

    public readonly static EventType<Event> AddedToStage = "addedToStage";
    public readonly static EventType<Event> RemovedFromStage = "removedFromStage";
    public readonly static EventType<Event> Added = "added";
    public readonly static EventType<Event> Removed = "removed";
    public readonly static EventType<Event> EnterFrame = "enterFrame";
}

/// <summary>
/// Keyboard events are *ONLY* dispatched on stage layer, if listeners are put on any other sprite they will not trigger!
/// </summary>
public class KeyboardEvent(EventType<KeyboardEvent> type, Key key, Scancode code, bool ctrl, bool shift, bool alt)
    : Event(type.Id, true, true) {
    public readonly Key Key = key;
    public readonly Scancode Code = code;
    public readonly bool Ctrl = ctrl;
    public readonly bool Shift = shift;
    public readonly bool Alt = alt;

    public readonly static EventType<KeyboardEvent> KeyDown = "keyDown";
    public readonly static EventType<KeyboardEvent> KeyUp = "keyUp";
}

public class FocusEvent(EventType<FocusEvent> type, Sprite relatedObject) : Event(type.Id, true) {
    public readonly Sprite RelatedObject = relatedObject;

    public readonly static EventType<FocusEvent> FocusIn = "focusIn";
    public readonly static EventType<FocusEvent> FocusOut = "focusOut";
}

public class MouseEvent(
    EventType<MouseEvent> type,
    Vector2i coords = new(),
    Vector2 delta = new(),
    bool shiftKey = false,
    bool ctrlKey = false,
    bool altKey = false) : Event(type.Id, true) {
    public readonly Vector2i Coords = coords;
    public readonly float VerticalDelta = delta.Y;
    public readonly float HorizontalDelta = delta.X;
    public readonly bool ShiftKey = shiftKey;
    public readonly bool CtrlKey = ctrlKey;
    public readonly bool AltKey = altKey;
    public readonly bool Captured;

    public MouseEvent(EventType<MouseEvent> type, Vector2i coords, Vector2 delta, bool shiftKey, bool ctrlKey, bool altKey, bool captured)
        : this(type, coords, delta, shiftKey, ctrlKey, altKey) {
        Captured = captured;
    }

    public readonly static EventType<MouseEvent> LeftClick = "leftClick";
    public readonly static EventType<MouseEvent> MiddleClick = "middleClick";
    public readonly static EventType<MouseEvent> RightClick = "rightClick";
    public readonly static EventType<MouseEvent> MouseOver = "mouseOver";
    public readonly static EventType<MouseEvent> MouseOut = "mouseOut";
    public readonly static EventType<MouseEvent> LeftDown = "leftDown";
    public readonly static EventType<MouseEvent> MiddleDown = "middleDown";
    public readonly static EventType<MouseEvent> RightDown = "rightDown";
    public readonly static EventType<MouseEvent> LeftUp = "leftUp";
    public readonly static EventType<MouseEvent> MiddleUp = "middleUp";
    public readonly static EventType<MouseEvent> RightUp = "rightUp";
    public readonly static EventType<MouseEvent> MouseMove = "mouseMove";
    public readonly static EventType<MouseEvent> ScrollVertical = "scrollVertical";
    public readonly static EventType<MouseEvent> ScrollHorizontal = "scrollHorizontal";

    private readonly static HashSet<EventType<MouseEvent>> ButtonTypes =
        [LeftClick, MiddleClick, RightClick, LeftDown, MiddleDown, RightDown, LeftUp, MiddleUp, RightUp, ScrollVertical, ScrollHorizontal];

    internal static bool IsButtonType(EventType<MouseEvent> type) => ButtonTypes.Contains(type);
}

/// <summary>
/// Resize events are *ONLY* dispatched on stage layer, if listeners are put on any other sprite they will not trigger!
/// </summary>
public class ResizeEvent(EventType<ResizeEvent> type, int width, int height) : Event(type.Id) {
    public readonly int Width = width;
    public readonly int Height = height;

    public readonly static EventType<ResizeEvent> Resize = "resize";
}