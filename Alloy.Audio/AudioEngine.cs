namespace Alloy.Audio;

public class AudioEngine {
    
    private readonly InternalAudioEngine _engine;
    private readonly CancellationTokenSource _cancelToken = new();
    private readonly Thread _audioThread;

    public AudioEngine(string localPath, string webPath, int maxSongSources = 4, int maxEffectSources = 28) {
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
        //TODO: add min delay check for repeating effects
        _engine.EnqueueCommand(new EngineCommand(AllTypes.EffectLocal, effect));
    }

    public void ClearCache(CacheType cache = CacheType.All) {
        _engine.EnqueueCommand(new EngineCommand((AllTypes)cache));
    }
}