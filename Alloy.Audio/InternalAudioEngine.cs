using System.Diagnostics;
using Alloy.Audio.Utils;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;
using StbVorbisSharp;

namespace Alloy.Audio;

internal class InternalAudioEngine {
    
    public const int TotalMaxSources = 32;
    private readonly string _localContentPath;
    private readonly string _webContentPath;
    private readonly SourcePool _songPool;
    private readonly SourcePool _effectPool;
    private readonly CancellationToken _cancelToken;
    private readonly Lock _commandLock = new();
    private readonly Queue<EngineCommand> _commandQueue = [];
    private readonly Dictionary<string, Vorbis> _oggCache = [];
    private readonly Dictionary<string, int> _effectCache = [];
    private readonly List<Music> _activeMusic = [];
    private readonly List<Effect> _activeEffect = [];
    
    private ALCDevice _currentDevice = ALCDevice.Null;
    private ALCContext _currentContext = ALCContext.Null;

    private float _gainMaster;
    private float _gainMusic;
    private float _gainEffect;
    private Music _currentSong;

    internal InternalAudioEngine(CancellationToken cancelToken, string localPath, string webPath, int maxSongSources = 4, int maxEffectSources = 28) {
        if (maxSongSources + maxEffectSources > TotalMaxSources) {
            throw new Exception($"Total audio sources must not exceed {TotalMaxSources}, had {maxSongSources + maxEffectSources}");
        }
        
        OpenALLibraryNameContainer.OverridePath = InternalUtils.GetAudioBinaryPath();

        _cancelToken = cancelToken;

        _localContentPath = localPath;
        _webContentPath = webPath;
        
        _songPool = new SourcePool(maxSongSources);
        _effectPool = new SourcePool(maxEffectSources);
    }

    public void EnqueueCommand(EngineCommand command) {
        using (_commandLock.EnterScope());
        _commandQueue.Enqueue(command);
    }

    public void Run() {
        // these dont need to be on audio thread
        var defaultDevice = ALC.GetDefaultDevice();

        _currentDevice = ALC.OpenDevice(defaultDevice);
        _currentContext = ALC.CreateContext(_currentDevice, []);
        ALC.MakeContextCurrent(_currentContext);
        
        _songPool.Initialize();
        _effectPool.Initialize();

        var stopwatch = Stopwatch.StartNew();
        var totalMs = 0d;
        var deltaMs = 0d;

        while (!_cancelToken.IsCancellationRequested) {
            HandleCommands(totalMs);
            
            deltaMs = stopwatch.Elapsed.TotalMilliseconds;
            totalMs += deltaMs;
            stopwatch.Restart();
            
            Loop(totalMs, deltaMs);
            
            Thread.Sleep(16);
        }
        
        // Cleanup
        ALC.MakeContextCurrent(ALCContext.Null);
        ALC.DestroyContext(_currentContext);
        ALC.CloseDevice(_currentDevice);
    }

    private void HandleCommands(double time) {
        using (_commandLock.EnterScope());

        while (_commandQueue.TryDequeue(out var command)) {
            switch (command.Type) {
                case AllTypes.GainMaster: AL.Listenerf(ListenerPNameF.Gain, _gainMaster = command.FloatValue); break;
                case AllTypes.GainMusic: _gainMusic = command.FloatValue; break;
                case AllTypes.GainEffect: _gainEffect = command.FloatValue; break;
                //case AllTypes.FadeIn: break;
                //case AllTypes.FadeOut: break;
                case AllTypes.MusicLocal: PlayLocalSong(command.Path, command.FloatValue, time); break;
                case AllTypes.EffectLocal: PlayLocalEffect(command.Path); break;
                //case AllTypes.MusicWeb: break;
                case AllTypes.ClearCache: ClearCache(command.Cache); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Loop(double totalMs, double deltaMs) {
        foreach (var music in _activeMusic) {
            music.Update(totalMs, _gainMusic);
            // TODO: remove stale tracks
        }
        
        foreach (var effect in _activeEffect) {
            effect.Update(totalMs, _gainMusic);
            // TODO: remove stale tracks
        }
    }

    private void ClearCache(CacheType source) {
        switch (source) {
            case CacheType.All:
                ClearOgg();
                ClearEffect();
                break;
            case CacheType.Music:
                ClearOgg();
                break;
            case CacheType.Effect:
                ClearEffect();
                break;
            default: throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }
    }

    private void ClearOgg() {
        foreach (var kvp in _oggCache) {
            kvp.Value.Dispose();
        }
        
        _oggCache.Clear();
    }

    private void ClearEffect() {
        // TODO: track buffers in use so we dont clear active ones
        foreach (var buffer in _effectCache) {
            AL.DeleteBuffer(buffer.Value);
        }
        
        _effectCache.Clear();
    }

    private void PlayLocalSong(string file, float fade, double time) {
        if (!_songPool.TryPop(out var source)) {
            Console.WriteLine($"Failed to play song {file}, no free sources");
            return;
        }
        
        if (!_oggCache.TryGetValue(file, out var vorbis)) {
            var path = Path.Combine(_localContentPath, file);

            if (Path.GetExtension(path) != ".ogg") {
                Console.WriteLine($"Failed to play song {path}, not an '.ogg' file");
                return;
            }

            if (!File.Exists(path)) {
                Console.WriteLine($"Failed to find song at {path}");
                return;
            }

            var data = File.ReadAllBytes(path);
            vorbis = Vorbis.FromMemory(data);
        }

        var song = new Music(vorbis, source);
        song.SetFade(time, fade, FadeType.In);
        _activeMusic.Add(song);
        
        _currentSong?.SetFade(time, fade, FadeType.Out);
        _currentSong?.EndAt(time + fade);
        _currentSong = song;
    }

    private void PlayLocalEffect(string file) {
        if (!_effectPool.TryPop(out var source)) {
            Console.WriteLine($"Failed to play song {file}, no free sources");
            return;
        }

        if (!_effectCache.TryGetValue(file, out var buffer)) {
            var path = Path.Combine(_localContentPath, file);
            
            if (Path.GetExtension(path) != ".ogg") {
                Console.WriteLine($"Failed to play song {path}, not an '.ogg' file");
                return;
            }
            
            if (!File.Exists(path)) {
                Console.WriteLine($"Failed to find song at {path}");
                return;
            }
            
            var fileData = File.ReadAllBytes(path);
            var data = StbVorbis.decode_vorbis_from_memory(fileData, out var sampleRate, out var channels);

            AL.GenBuffer(out buffer);
            AL.BufferData(buffer, InternalUtils.GetChannelFormat(channels), ref data[0], data.Length * sizeof(short), sampleRate);
            _effectCache[file] = buffer;
        }

        var effect = new Effect(source, buffer);
        _activeEffect.Add(effect);
    }
}