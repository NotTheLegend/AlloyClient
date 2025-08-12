using System;

namespace RealmClient;

public enum GraphicsOptions {
    TitleScreen,
    InGame
}

[Flags]
public enum InputBlockers {
    None = 0,
    Chat = 1 << 1,
    Panel = 1 << 2,
    Dialog = 1 << 3,
}

public enum SpeechColors {
    Default,
    Enemy,
    Guild,
    Party,
    Tell
}

public enum UseType {
    DEFAULT = 0,
    START_USE = 1,
    END_USE = 2
}