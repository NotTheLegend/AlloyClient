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
    public static implicit operator EventType<T>(string id) => new(id);
    public static implicit operator string(EventType<T> type) => type.Id;
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

public class MouseEvent(EventType<MouseEvent> type, Vector2i coords = new (), Vector2 delta = new (), bool shiftKey = false, bool ctrlKey = false, bool altKey = false) : Event(type.Id, true) {
    public readonly Vector2i Coords = coords;
    public readonly float VerticalDelta = delta.Y;
    public readonly float HorizontalDelta = delta.X;
    public readonly bool ShiftKey = shiftKey;
    public readonly bool CtrlKey = ctrlKey;
    public readonly bool AltKey = altKey;

    public static readonly EventType<MouseEvent> LeftClick = "leftClick";
    public static readonly EventType<MouseEvent> MiddleClick = "middleClick";
    public static readonly EventType<MouseEvent> RightClick = "rightClick";
    public static readonly EventType<MouseEvent> MouseOver = "mouseOver";
    public static readonly EventType<MouseEvent> MouseOut = "mouseOut";
    public static readonly EventType<MouseEvent> LeftDown = "leftDown";
    public static readonly EventType<MouseEvent> MiddleDown = "middleDown";
    public static readonly EventType<MouseEvent> RightDown = "rightDown";
    public static readonly EventType<MouseEvent> LeftUp = "leftUp";
    public static readonly EventType<MouseEvent> MiddleUp = "middleUp";
    public static readonly EventType<MouseEvent> RightUp = "rightUp";
    public static readonly EventType<MouseEvent> MouseMove = "mouseMove";
    public static readonly EventType<MouseEvent> ScrollVertical = "scrollVertical";
    public static readonly EventType<MouseEvent> ScrollHorizontal = "scrollHorizontal";

    private static readonly HashSet<EventType<MouseEvent>> ButtonTypes = [LeftClick, MiddleClick, RightClick, LeftDown, MiddleDown, RightUp, LeftUp, MiddleUp, RightUp, ScrollVertical, ScrollHorizontal];
    
    internal static bool IsButtonType(EventType<MouseEvent> type) => ButtonTypes.Contains(type);
}

/// <summary>
/// Resize events are *ONLY* dispatched on stage layer, if listeners are put on any other sprite they will not trigger!
/// </summary>
public class ResizeEvent(EventType<ResizeEvent> type, int width, int height) : Event(type.Id) {
    public readonly int Width = width;
    public readonly int Height = height;
    
    public static readonly EventType<ResizeEvent> Resize = "resize";
}