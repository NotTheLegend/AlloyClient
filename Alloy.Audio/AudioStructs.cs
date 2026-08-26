using System.Runtime.InteropServices;
using StbVorbisSharp;

namespace Alloy.Audio;

public readonly record struct FadeState(AudioFade Fade, double DurationMs) {
    public readonly static FadeState Default = new(AudioFade.In, 0f);

    public static FadeState Out(double durationMs) => new FadeState(AudioFade.Out, durationMs);
    
    public static FadeState In(double durationMs) => new FadeState(AudioFade.In, durationMs);
}

internal struct EngineCommand {
    public AudioAll Type;
    public string FilePath;
    public FadeState Fade;
    public int ChannelId;
    public int TrackId;
    public float Volume;
    public double DurationMs;
    public AudioMode Mode;
    public AudioState State;
    
    internal static EngineCommand CreateMasterVolume(float volume) {
        return new EngineCommand {
            Type = AudioAll.GainMaster,
            Volume = volume
        };
    }

    internal static EngineCommand CreateChannelVolume(int channelId, float volume) {
        return new EngineCommand {
            Type = AudioAll.GainChannel,
            ChannelId = channelId,
            Volume = volume
        };
    }

    internal static EngineCommand CreateTrackVolume(int channelId, int trackId, float volume) {
        return new EngineCommand {
            Type = AudioAll.GainTrack,
            ChannelId = channelId,
            TrackId = trackId,
            Volume = volume
        };
    }

    internal static EngineCommand CreatePlay(string name, int channelId, int trackId, AudioMode mode, AudioState state, FadeState fade) {
        return new EngineCommand {
            Type = AudioAll.Play,
            ChannelId = channelId,
            TrackId = trackId,
            FilePath = name,
            Mode = mode,
            State = state,
            Fade = fade
        };
    }

    internal static EngineCommand CreateStop(int channelId, int trackId, double durationMs) {
        return new EngineCommand {
            Type = AudioAll.Stop,
            ChannelId = channelId,
            TrackId = trackId,
            DurationMs = durationMs
        };
    }

    internal static EngineCommand CreateFade(int channelId, int trackId, FadeState fade) {
        return new EngineCommand {
            Type = AudioAll.Fade,
            Fade = fade,
            ChannelId = channelId,
            TrackId = trackId,
        };
    }
}

[StructLayout(LayoutKind.Explicit)]
internal readonly struct TrackLookup(int channelId, int trackId) {
    [FieldOffset(0)] public readonly long Id;
    [FieldOffset(0)] public readonly int ChannelId = channelId;
    [FieldOffset(4)] public readonly int TrackId = trackId;
}

internal class StaticBuffer(int id) {
    public readonly int Id = id;
    public int ActiveCount;
}

internal class StreamBuffer(string filePath, Vorbis vorbis) {
    public readonly string FilePath = filePath;
    public readonly Vorbis Vorbis = vorbis;
}