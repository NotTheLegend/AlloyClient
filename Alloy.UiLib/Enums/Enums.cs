using System;

namespace Alloy.UiLib.Enums;

public enum UiAnchor : byte {
    LeftTop = 0,
    MiddleTop = 1,
    RightTop = 2,
    MiddleLeft = 3,
    Middle = 4,
    MiddleRight = 5,
    LeftBottom = 6,
    MiddleBottom = 7,
    RightBottom = 8
}

public enum TextureType : byte {
    None = 255,
    Color = 0,
    GameAtlas = 1,
    UiAtlas = 2,
    UiAtlasLinear = 3,
    UiSlice = 4,
    Text = 5,
    TitleBackground = 6,
    TitleGraphic = 7,
    Minimap = 8,
    Ellipse = 9,
}

public enum CollisionType : byte {
    Square,
    Ellipse,
    Vertices,
    Custom,
    CustomNoScale,
}

[Flags]
public enum CutEdges : uint {
    None = 0,
    TopLeft = 1 << 1,
    TopRight = 1 << 2,
    BottomRight = 1 << 3,
    BottomLeft = 1 << 4,
    Left = TopLeft | BottomLeft,
    Right = TopRight | BottomRight,
    Top = TopLeft | TopRight,
    Bottom = BottomLeft | BottomRight,
    All = Top | Bottom
}

public enum FontType : int {
    Normal = 0,
    Bold = 1,
    Bolder = 2
}

public enum TaskState {
    Completed,
    Faulted,
    Canceled
}