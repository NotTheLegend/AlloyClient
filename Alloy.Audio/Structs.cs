namespace Alloy.Audio;

internal record struct MusicCommand(ContentType Type, string Path, float FadeDuration);