using System.Diagnostics;

namespace Alloy.Audio;

public abstract class AudioChannel {
    
    public const double DefaultDelayMs = 1000d / 15;

    public double MiniumRepeatDelayMs { get; set => field = Math.Max(value, 0d); } = DefaultDelayMs;
    
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly Dictionary<string, double> _lastPlayed = [];

    private int _audioIdGenerator = 1;
    private InternalAudioEngine _audioEngine;
    private int _channelId;

    internal void Register(InternalAudioEngine audioEngine, int channelId) {
        if (_channelId > 0) {
            throw new Exception("Audio channel was already registered");
        }
        
        _audioEngine = audioEngine;
        _channelId = channelId;
    }

    public void SetVolume(float volume) => _audioEngine.EnqueueCommand(EngineCommand.CreateChannelVolume(_channelId, Math.Clamp(volume, 0f, 1f)));
    
    public void SetVolumeTrack(float volume, int trackId) => _audioEngine.EnqueueCommand(EngineCommand.CreateTrackVolume(_channelId, trackId, Math.Clamp(volume, 0f, 1f)));

    public void PlayTrack(string name, AudioMode mode, AudioState state, out int trackId) => trackId = PlayTrack(name, mode, state);
    
    public void PlayTrack(string name, AudioMode mode, AudioState state, FadeState fade, out int trackId) => trackId = PlayTrack(name, mode, state, fade);

    public int PlayTrack(string name, AudioMode mode, AudioState state) => PlayTrack(name, mode, state, FadeState.Default);
    
    public int PlayTrack(string name, AudioMode mode, AudioState state, FadeState fade) {
        _lastPlayed.TryGetValue(name, out var time);
        
        if (time + MiniumRepeatDelayMs > _stopwatch.Elapsed.TotalMilliseconds) {
            return -1;
        }

        var id = _audioIdGenerator++;
        _lastPlayed[name] = _stopwatch.Elapsed.TotalMilliseconds;
        _audioEngine.EnqueueCommand(EngineCommand.CreatePlay(name, _channelId, id, mode, state, fade));
        return id;
    }

    public void StopTrack(int trackId) => _audioEngine.EnqueueCommand(EngineCommand.CreateStop(_channelId, trackId, 0));
    
    public void StopTrackIn(int trackId, double durationMs) => _audioEngine.EnqueueCommand(EngineCommand.CreateStop(_channelId, trackId, durationMs));
    
    public void SetFadeTrack(int trackId, FadeState fade) => _audioEngine.EnqueueCommand(EngineCommand.CreateFade(_channelId, trackId, fade));
}

public class SingleTrackChannel : AudioChannel {

    private int _lastPlayedTrack;
    
    public void Play(string name) {
        StopTrack(_lastPlayedTrack);
        PlayTrack(name, AudioMode.Stream, AudioState.Loop, out _lastPlayedTrack);
    }

    public void FadeTo(string name, float durationSeconds) {
        var durationMs = durationSeconds * 1000d;
        SetFadeTrack(_lastPlayedTrack, FadeState.Out(durationMs));
        StopTrackIn(_lastPlayedTrack, durationMs);
        PlayTrack(name, AudioMode.Stream, AudioState.Loop, FadeState.In(durationMs), out _lastPlayedTrack);
    }


    public void FadeTo(string name, float fadeOutDurationSeconds, float fadeInDurationSeconds) {
        SetFadeTrack(_lastPlayedTrack, FadeState.Out(fadeOutDurationSeconds * 1000d));
        StopTrackIn(_lastPlayedTrack, fadeInDurationSeconds * 1000d);
        PlayTrack(name, AudioMode.Stream, AudioState.Loop, FadeState.In(fadeInDurationSeconds * 1000d), out _lastPlayedTrack);
    }
}

public class SfxChannel : AudioChannel {

    public void Play(string name) => PlayTrack(name, AudioMode.Static, AudioState.FireAndForget);
}

