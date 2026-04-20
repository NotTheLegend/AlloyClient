using System.Collections.Concurrent;
using System.Diagnostics;
using Alloy.Audio.Utils;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;
using StbVorbisSharp;

namespace Alloy.Audio;

public class AudioEngine {

    internal const int TotalMaxSources = 32;
    internal readonly int MaxSongSources;
    internal readonly int MaxEffectSources;

    internal readonly string LocalContentPath;
    internal readonly string WebContentPath;
    
    private string _defaultDevice;
    private string[] _allDevices;

    private readonly Thread _audioThread;

    private readonly SourcePool _songPool;
    private readonly SourcePool _effectPool;

    private readonly ConcurrentQueue<MusicCommand> _songCommands = [];

    private readonly Dictionary<string, Vorbis> _oggCache = [];

    private readonly List<Music> _activeMusic = [];

    private ALCDevice _currentDevice = ALCDevice.Null;
    private ALCContext _currentContext = ALCContext.Null;

    private volatile bool _stop;

    public AudioEngine(string localPath, string webPath, int maxSongSources = 4, int maxEffectSources = 28) {
        if (maxSongSources + maxEffectSources > TotalMaxSources) {
            throw new Exception($"Total audio sources must not exceed {TotalMaxSources}, had {maxSongSources + maxEffectSources}");
        }

        LocalContentPath = localPath;
        WebContentPath = webPath;

        MaxSongSources = maxSongSources;
        MaxEffectSources = maxEffectSources;
        
        _songPool = new SourcePool(maxSongSources);
        _effectPool = new SourcePool(maxEffectSources);

        _audioThread = new Thread(Run);
    }

    public void Start() {
        _audioThread.Start();
    }

    public void StopAndDispose() {
        _stop = true;
        _audioThread.Join();
        ALC.MakeContextCurrent(ALCContext.Null);
        ALC.DestroyContext(_currentContext);
        ALC.CloseDevice(_currentDevice);
    }

    public void PlayLocalMusic(string path, float fadeDuration = 2f) {
        path = Path.Combine(LocalContentPath, path);

        if (Path.GetExtension(path) != ".ogg") {
            Console.WriteLine($"Failed to play song {path}, not an '.ogg' file");
            return;
        }

        if (!File.Exists(path)) {
            Console.WriteLine($"Failed to find song at {path}");
            return;
        }
        
        _songCommands.Enqueue(new MusicCommand(ContentType.Local, path, fadeDuration * 1000));
    }

    public void Run() {
        OpenALLibraryNameContainer.OverridePath = InternalUtils.GetAudioBinaryPath();

        _defaultDevice = ALC.GetDefaultDevice();
        _allDevices = ALC.GetAllDevices();

        _currentDevice = ALC.OpenDevice(_defaultDevice);
        _currentContext = ALC.CreateContext(_currentDevice, []);
        ALC.MakeContextCurrent(_currentContext);
        
        _songPool.Initialize();
        _effectPool.Initialize();

        var stopwatch = Stopwatch.StartNew();
        var totalMs = 0d;
        var deltaMs = 0d;

        while (!_stop) {
            // handle commands here
            while (_songCommands.TryDequeue(out var command)) {
                using var stream = File.OpenRead(command.Path);
                var data = new byte[stream.Length];
                stream.ReadExactly(data);

                var vorbis = Vorbis.FromMemory(data);

                var song = new Music(vorbis, _songPool.Pop());
                song.SetFade(totalMs, command.FadeDuration, FadeType.In);
                _activeMusic.Add(song);
            }
            
            deltaMs = stopwatch.Elapsed.TotalMilliseconds;
            totalMs += deltaMs;
            stopwatch.Restart();
            
            Loop(totalMs, deltaMs);
            
            Thread.Sleep(16);
        }
    }

    private void Loop(double totalMs, double deltaMs) {
        foreach (var music in _activeMusic) {
            music.Update(totalMs);
        }
    }

    public string GetDefaultDevice() => _defaultDevice;
    
    public ReadOnlySpan<string> GetAllDevices() => _allDevices;
}