using System;

namespace Alloy.UiLib.Core;

public readonly record struct UiAnchor { // enums dont extend IEquatable, hence this hack

    private readonly byte _value;

    private UiAnchor(byte value) {
        _value = value;
    }

    public static implicit operator UiAnchor(byte type) => new(type);

    public static implicit operator byte(UiAnchor anchor) => anchor._value;

    public static readonly UiAnchor LeftTop = 0;
    public static readonly UiAnchor MiddleTop = 1;
    public static readonly UiAnchor RightTop = 2;
    public static readonly UiAnchor MiddleLeft = 3;
    public static readonly UiAnchor Middle = 4;
    public static readonly UiAnchor MiddleRight = 5;
    public static readonly UiAnchor LeftBottom = 6;
    public static readonly UiAnchor MiddleBottom = 7;
    public static readonly UiAnchor RightBottom = 8;

    internal (int, int) GetOffset(int w, int h) => _value switch {
        0 => (0, 0),
        1 => (-w / 2, 0),
        2 => (-w, 0),
        3 => (0, -h / 2),
        4 => (-w / 2, -h / 2),
        5 => (-w, -h / 2),
        6 => (0, -h),
        7 => (-w / 2, -h),
        8 => (-w, -h),
        _ => (0, 0)
    };
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