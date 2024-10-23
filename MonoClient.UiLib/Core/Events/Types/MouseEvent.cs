using System;
using Common.Vector;

namespace MonoClient.UiLib.Core.Events.Types;

[Flags]
public enum MouseEventId : int {
    LeftClick = 1 << 1,
    MiddleClick = 1 << 2,
    RightClick = 1 << 3,
    MouseOver = 1 << 4,
    MouseOut = 1 << 5,
    LeftDown = 1 << 6,
    MiddleDown = 1 << 7,
    RightDown = 1 << 8,
    LeftUp = 1 << 9,
    MiddleUp = 1 << 10,
    RightUp = 1 << 11,
    MouseMove = 1 << 12,
    Scroll = 1 << 13,
}

internal record MouseEventData(MouseEventId EventId, Delegate Callback, bool Consume, bool Global);

public record MouseEventArgs(Sprite Sprite, IntVector2 Coords = new(), float Delta = 0f);