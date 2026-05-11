using System.Diagnostics;

namespace Alloy.Audio;

public class AudioEngine {
    public const int TotalMaxSources = 32;
    
    private readonly InternalAudioEngine _engine;
    private readonly CancellationTokenSource _cancelToken = new();
    private readonly Thread _audioThread;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly Dictionary<string, double> _lastPlayed = [];
    private readonly double _delay;

    public AudioEngine(string localPath, string webPath, double minimumEffectDelayMs = 15d, int maxSongSources = 4, int maxEffectSources = 28) {
        if (maxSongSources + maxEffectSources > TotalMaxSources) {
            throw new Exception($"Total audio sources must not exceed {TotalMaxSources}, had {maxSongSources + maxEffectSources}");
        }
        
        _delay = minimumEffectDelayMs;
        _engine = new InternalAudioEngine(_cancelToken.Token, localPath, webPath, maxSongSources, maxEffectSources);
        _audioThread = new Thread(_engine.Run) {
            IsBackground = true,
            Name = "Alloy.Audio.Engine",
            Priority = ThreadPriority.Normal
        };
    }
    
    public void Start() {
        _audioThread.Start();
    }
    
    public void StopAndDispose() {
        _cancelToken.Cancel();
        _audioThread.Join();
        _cancelToken.Dispose();
    }
    
    public void SetVolume(AudioSource source, float volume) {
        volume = Math.Clamp(volume, 0f, 1f);
        switch (source) {
            case AudioSource.Master: _engine.EnqueueCommand(new EngineCommand(AllTypes.GainMaster, volume)); break;
            case AudioSource.Music: _engine.EnqueueCommand(new EngineCommand(AllTypes.GainMusic, volume)); break;
            case AudioSource.Effect: _engine.EnqueueCommand(new EngineCommand(AllTypes.GainEffect, volume)); break;
            default: throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }
    }

    public void PlayLocalSong(string song, float fadeDuration = 2f) {
        //TODO: move file checks here, and optional .ogg to song
        _engine.EnqueueCommand(new EngineCommand(AllTypes.MusicLocal, fadeDuration * 1000f, song));
    }
    
    public void PlayLocalEffect(string effect) {
        //TODO: move file checks here, and optional .ogg to song

        if (!LastPlayCheck(effect)) {
            return;
        }
        _engine.EnqueueCommand(new EngineCommand(AllTypes.EffectLocal, effect));
    }

    public void ClearCache(CacheType cache = CacheType.All) {
        _engine.EnqueueCommand(new EngineCommand((AllTypes)cache));
    }

    private bool LastPlayCheck(string sfx) {
        if (_lastPlayed.TryGetValue(sfx, out var time)) {
            if (time + _delay > _stopwatch.Elapsed.TotalMilliseconds) {
                return false;
            }
        }

        _lastPlayed[sfx] = _stopwatch.Elapsed.TotalMilliseconds;
        return true;
    }
}