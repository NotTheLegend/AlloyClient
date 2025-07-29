using Common.Vector;
using OpenTK.Platform;

namespace MonoClient.UiLib.Core;

public record Event(string Type, bool Bubbles = false) {
    
    public Sprite Target { get; private set; }
    
    public Sprite CurrentTarget { get; private set; }

    internal bool Stop;
    
    internal bool ImmediateStop;

    internal void SetTarget(Sprite target) => Target = target;
    internal void SetCurrentTarget(Sprite target) => CurrentTarget = target;

    public void StopPropagation() => Stop = true;
    
    public void StopImmediatePropagation() => ImmediateStop = true;
    
    public const string AddedToStage = "addedToStage";
    public const string RemovedToStage = "removedToStage";
}

/// <summary>
/// Keyboard events are *ONLY* dispatched on stage layer, if listeners are put on any other sprite they will not trigger!
/// </summary>
public record KeyboardEvent(string Type, Key Key, Scancode Code, bool Ctrl, bool Shift, bool Alt) : Event(Type, true) {
    public const string KeyDown = "keyDown";
    public const string KeyUp = "keyUp";

    internal static bool ValidateType(string type) {
        return type switch {
            KeyDown => true,
            KeyUp => true,
            _ => false
        };
    }
}

public record MouseEvent(string Type, IntVector2 Coords = new(), float Delta = 0f, bool ShiftKey = false, bool CtrlKey = false, bool AltKey = false) : Event(Type, true) {
    public const string LeftClick = "leftClick";
    public const string MiddleClick = "middleClick";
    public const string RightClick = "rightClick";
    public const string MouseOver = "mouseOver";
    public const string MouseOut = "mouseOut";
    public const string LeftDown = "leftDown";
    public const string MiddleDown = "middleDown";
    public const string RightDown = "rightDown";
    public const string LeftUp = "leftUp";
    public const string MiddleUp = "middleUp";
    public const string RightUp = "rightUp";
    public const string MouseMove = "mouseMove";
    public const string ScrollVertical = "scrollVertical";
    public const string ScrollHorizontal = "scrollHorizontal";
    
    internal static bool ValidateType(string type) {
        return type switch {
            LeftClick => true,
            MiddleClick => true,
            RightClick => true,
            MouseOver => true,
            MouseOut => true,
            LeftDown => true,
            MiddleDown => true,
            RightDown => true,
            LeftUp => true,
            MiddleUp => true,
            RightUp => true,
            MouseMove => true,
            ScrollVertical => true,
            ScrollHorizontal => true,
            _ => false
        };
    }
    
    internal static bool IsButtonType(string type) {
        return type switch {
            LeftClick => true,
            MiddleClick => true,
            RightClick => true,
            LeftDown => true,
            MiddleDown => true,
            RightDown => true,
            LeftUp => true,
            MiddleUp => true,
            RightUp => true,
            ScrollVertical => true,
            ScrollHorizontal => true,
            _ => false
        };
    }
}

/// <summary>
/// Resize events are *ONLY* dispatched on stage layer, if listeners are put on any other sprite they will not trigger!
/// </summary>
public record ResizeEvent(string Type, int Width, int Height) : Event(Type) {
    public const string Resize = "resize";

    internal static bool ValidateType(string type) {
        return type switch {
            Resize => true,
            _ => false
        };
    }
}