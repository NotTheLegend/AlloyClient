using System;

namespace MonoClient;

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