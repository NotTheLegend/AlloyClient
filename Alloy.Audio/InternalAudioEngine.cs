using System.Diagnostics;
using Alloy.Audio.Utils;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;
using StbVorbisSharp;

namespace Alloy.Audio;

internal class InternalAudioEngine {
    
    private readonly string _localContentPath;
    private readonly string _webContentPath; // TODO: web loaded sound
    private readonly SourcePool<Music> _songPool;
    private readonly SourcePool<Effect> _effectPool;
    private readonly CancellationToken _cancelToken;
    private readonly Lock _commandLock = new();
    private readonly Queue<EngineCommand> _commandQueue = [];
    private readonly Dictionary<string, Vorbis> _oggCache = [];
    private readonly Dictionary<string, int> _effectCache = [];
    private readonly List<Music> _activeMusic = [];
    private readonly List<Effect> _activeEffect = [];
    
    private ALCDevice _currentDevice = ALCDevice.Null;
    private ALCContext _currentContext = ALCContext.Null;

    private float _gainMaster = 1f;
    private float _gainMusic = 1f;
    private float _gainEffect = 1f;
    private Music _currentSong;

    internal InternalAudioEngine(CancellationToken cancelToken, string localPath, string webPath, int maxSongSources = 4, int maxEffectSources = 28) {
        OpenALLibraryNameContainer.OverridePath = InternalUtils.GetAudioBinaryPath();

        _cancelToken = cancelToken;

        _localContentPath = localPath;
        _webContentPath = webPath;
        
        _songPool = new SourcePool<Music>(maxSongSources);
        _effectPool = new SourcePool<Effect>(maxEffectSources);
    }

    public void EnqueueCommand(EngineCommand command) {
        using (_commandLock.EnterScope());
        _commandQueue.Enqueue(command);
    }

    public void Run() {
        //TODO: load/save device to settings
        var defaultDevice = ALC.GetDefaultDevice();

        _currentDevice = ALC.OpenDevice(defaultDevice);
        _currentContext = ALC.CreateContext(_currentDevice, []);
        ALC.MakeContextCurrent(_currentContext);
        
        InitPools();

        var stopwatch = Stopwatch.StartNew();
        var totalMs = 0d;

        while (!_cancelToken.IsCancellationRequested) {
            HandleCommands(totalMs);
            
            totalMs += stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            
            Loop(totalMs);
            
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

    private void Loop(double totalMs) {
        foreach (var music in _activeMusic) {
            music.Update(totalMs, _gainMusic);
        } // im doing this the lazy way
        _activeMusic.RemoveAll(music => { var stale = music.Stale; if (stale) { _songPool.Push(music); } return stale; });
        
        foreach (var effect in _activeEffect) {
            effect.Update(totalMs, _gainEffect);
        } // im doing this the lazy way (again)
        _activeEffect.RemoveAll(effect => { var stale = effect.Stale; if (stale) { _effectPool.Push(effect); } return stale; });
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
        if (!_songPool.TryPop(out var music)) {
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

        music.SetVorbis(vorbis);
        music.SetFade(time, fade, FadeType.In);
        music.Play();
        _activeMusic.Add(music);
        
        _currentSong?.SetFade(time, fade, FadeType.Out);
        _currentSong?.EndAt(time + fade);
        _currentSong = music;
    }

    private void PlayLocalEffect(string file) {
        if (!_effectPool.TryPop(out var effect)) {
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

        effect.SetBuffer(buffer);
        effect.Play();
        _activeEffect.Add(effect);
    }

    private void InitPools() {
        for (var i = 0; i < _songPool.Capacity; i++) {
            _songPool.Push(new Music());
        }
        
        for (var i = 0; i < _effectPool.Capacity; i++) {
            _effectPool.Push(new Effect());
        }
    }
}