namespace Alloy.Audio;

internal enum AllTypes {
    None,
    GainMaster,
    GainMusic,
    GainEffect,
    FadeIn,
    FadeOut,
    MusicLocal,
    MusicWeb,
    EffectLocal,
    EffectWeb,
    ClearCache,
}

internal enum FadeType {
    In = AllTypes.FadeIn, 
    Out = AllTypes.FadeOut
}

public enum AudioSource {
    Master = AllTypes.GainMaster,
    Music = AllTypes.GainMusic,
    Effect = AllTypes.GainEffect,
}

public enum CacheType {
    All,
    Music,
    Effect
}