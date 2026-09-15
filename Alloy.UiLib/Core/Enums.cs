using System;

namespace Alloy.UiLib.Core;

public readonly record struct UiAnchor { // enums dont extend IEquatable, hence this hack

    internal readonly byte Value;

    private UiAnchor(byte value) {
        Value = value;
    }

    public static implicit operator UiAnchor(byte type) => new(type);

    public static implicit operator byte(UiAnchor anchor) => anchor.Value;

    public static readonly UiAnchor Default = 0;
    public static readonly UiAnchor TopLeft = 1;
    public static readonly UiAnchor TopMiddle = 2;
    public static readonly UiAnchor TopRight = 3;
    public static readonly UiAnchor MiddleLeft = 4;
    public static readonly UiAnchor Middle = 5;
    public static readonly UiAnchor MiddleRight = 6;
    public static readonly UiAnchor BottomLeft = 7;
    public static readonly UiAnchor BottomMiddle = 8;
    public static readonly UiAnchor BottomRight = 9;
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

/// <summary>
/// SimpleSquare only check bounds and skips children, the others check against self then loop children
/// </summary>
public enum CollisionType : byte {
    SimpleSquare,
    Square,
    Ellipse,
    Vertices,
    Custom
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