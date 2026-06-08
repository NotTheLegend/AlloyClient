namespace Alloy.Audio;

internal enum AudioAll {
    GainMaster,
    GainChannel,
    GainTrack,
    Play,
    Stop,
    Fade,
    ClearCache,
}

public enum AudioMode {
    Stream,
    Static,
}

public enum AudioState {
    FireAndForget,
    Loop,
}

public enum AudioFade {
    In,
    Out
}