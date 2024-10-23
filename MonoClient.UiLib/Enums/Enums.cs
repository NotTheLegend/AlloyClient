namespace MonoClient.UiLib.Enums;

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
    UiSlice = 3,
    TextNormal = 4,
    TextBold = 5,
    TitleBackground = 6,
    TitleGraphic = 7,
    Minimap = 8,
    Ellipse = 9,
}

public enum HitboxType : byte {
    Default,
    Ellipse,
    Complex,
    Custom
}