using System.Diagnostics;
using Alloy.Audio.Utils;
using Alloy.Common;
using Microsoft.Extensions.Logging;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;
using StbVorbisSharp;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Alloy.Audio;

public class AudioEngine {

    private readonly ILogger _log;

    private readonly InternalAudioEngine _audioEngine;
    private readonly Thread _audioThread;
    private readonly CancellationTokenSource _cancelToken = new();

    private int _channelIdGenerator = 1;

    public AudioEngine(ILoggerFactory logFactory, string localPath) {
        OpenALLibraryNameContainer.OverridePath = InternalUtils.GetAudioBinaryPath();
        
        _log = logFactory.CreateLogger("Alloy.Audio.Engine");
        _audioEngine = new InternalAudioEngine(_log, localPath, _cancelToken.Token);
        _audioThread = new Thread(_audioEngine.Run) {
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

    public void RegisterChannel(AudioChannel channel) {
        channel.Register(_audioEngine, _channelIdGenerator++);
    }

    public SingleTrackChannel CreateSingleTrackChannel() {
        var channel = new SingleTrackChannel();
        channel.Register(_audioEngine, _channelIdGenerator++);
        return channel;
    }
    
    public SfxChannel CreateSfxChannel() {
        var channel = new SfxChannel();
        channel.Register(_audioEngine, _channelIdGenerator++);
        return channel;
    }

    public void SetMasterVolume(float volume) => _audioEngine.EnqueueCommand(EngineCommand.CreateMasterVolume(Math.Clamp(volume, 0f, 1f)));
}

internal class InternalAudioEngine {

    private readonly ILogger _log;
    private readonly string _localContentPath;
    
    private readonly CancellationToken _cancelToken;
    private readonly Lock _commandLock = new();
    private readonly Queue<EngineCommand> _commandQueue = [];
    
    private ALCDevice _currentDevice = ALCDevice.Null;
    private ALCContext _currentContext = ALCContext.Null;

    private readonly Dictionary<int, float> _channelVolumes = [];
    private readonly Dictionary<TrackLookup, Track> _tracks = [];
    private readonly Dictionary<string, StaticBuffer> _staticBuffers = [];
    private readonly Dictionary<string, StreamBuffer> _vorbisBuffer = [];

    public InternalAudioEngine(ILogger log, string localContentPath, CancellationToken cancelToken) {
        _log = log;
        _localContentPath = localContentPath;
        _cancelToken = cancelToken;
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
        
        for (var i = 0; i < Pools.Sources.Capacity; i++) {
            Pools.Sources.Push(AL.GenSource());
        }
        
        var stopwatch = Stopwatch.StartNew();
        var totalMs = 0d;
        
        Thread.Sleep(16);

        while (!_cancelToken.IsCancellationRequested) {
            totalMs += stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            
            HandleCommands(totalMs);
            Tick(totalMs);
            
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
                case AudioAll.GainMaster: AL.Listenerf(ListenerPNameF.Gain, InternalUtils.GetLogVolume(command.Volume)); break;
                case AudioAll.GainChannel: _channelVolumes[command.ChannelId] = InternalUtils.GetLogVolume(command.Volume); break;
                case AudioAll.GainTrack: _tracks.GetValueOrDefault(new TrackLookup(command.ChannelId, command.TrackId))?.SetVolume(InternalUtils.GetLogVolume(command.Volume)); break;
                case AudioAll.Play: AddTrack(command, time); break;
                case AudioAll.Stop: _tracks.GetValueOrDefault(new TrackLookup(command.ChannelId, command.TrackId))?.Stop(time + command.DurationMs); break;
                case AudioAll.Fade: _tracks.GetValueOrDefault(new TrackLookup(command.ChannelId, command.TrackId))?.SetFadeState(command.Fade, time); break;
                case AudioAll.ClearCache: ClearCache(); break;
                default: throw new ArgumentOutOfRangeException(nameof(command), command.Type, "Not a valid engine command"); 
            }
        }
    }

    private void Tick(double time) {
        foreach (var (key, track) in _tracks) {
            _channelVolumes.TryGetValue(key.ChannelId, out var channelVolume);
            if (track.Update(time, channelVolume)) {
                continue;
            }
            
            track.Clear();
            switch (track) {
                case StreamTrack streamTrack: {
                    var vorbis = streamTrack.ClearVorbis();

                    if (!_vorbisBuffer.TryAdd(vorbis.FilePath, vorbis)) {
                        vorbis.Vorbis.Dispose();
                    }
                
                    Pools.StreamTracks.Push(streamTrack);
                    break;
                }
                case StaticTrack staticTrack:
                    Pools.StaticTracks.Push(staticTrack);
                    break;
            }

            _tracks.Remove(key);
        }
    }

    private void AddTrack(EngineCommand command, double time) {
        if (!Pools.Sources.TryPop(out var source)) {
            _log.Log(LogLevel.Information, $"Failed to play [{command.FilePath}], no available sources");
            return;
        }
        
        Track track;

        switch (command.Mode) {
            case AudioMode.Stream:
                track = CreateStreamTrack(command, source);
                break;
            case AudioMode.Static:
                track = CreateStaticTrack(command, source);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(command.Mode), command.Mode, "Not a valid track mode"); 
        }

        if (track is null) {
            return;
        }
        
        track.SetAudioState(command.State);
        track.SetFadeState(command.Fade, time);
        track.Play();

        _tracks[new TrackLookup(command.ChannelId, command.TrackId)] = track;
    }

    private StaticTrack CreateStaticTrack(EngineCommand command, int source) {
        Pools.StaticTracks.Pop(out var track);
        
        if (!_staticBuffers.TryGetValue(command.FilePath, out var buffer)) {
            var path = Path.CombineAlt(_localContentPath, command.FilePath);
            
            if (Path.GetExtension(path) != ".ogg") {
                _log.Log(LogLevel.Information, $"Failed to play song {path}, not an '.ogg' file");
                return null;
            }
            
            if (!File.Exists(path)) {
                _log.Log(LogLevel.Information, $"Failed to find song at {path}");
                return null;
            }
            
            var fileData = File.ReadAllBytes(path);
            var data = StbVorbis.decode_vorbis_from_memory(fileData, out var sampleRate, out var channels);

            AL.GenBuffer(out var id);
            AL.BufferData(id, InternalUtils.GetChannelFormat(channels), ref data[0], data.Length * sizeof(short), sampleRate);
            _staticBuffers[command.FilePath] = buffer = new StaticBuffer(id);
        }


        track.Setup(source, buffer);
        return track;
    }

    private StreamTrack CreateStreamTrack(EngineCommand command, int source) {
        Pools.StreamTracks.Pop(out var track);
        
        if (!_vorbisBuffer.Remove(command.FilePath, out var vorbis)) {
            var path = Path.CombineAlt(_localContentPath, command.FilePath);
            
            if (Path.GetExtension(path) != ".ogg") {
                _log.Log(LogLevel.Information, $"Failed to play song {path}, not an '.ogg' file");
                return null;
            }
            
            if (!File.Exists(path)) {
                _log.Log(LogLevel.Information, $"Failed to find song at {path}");
                return null;
            }
            
            var fileData = File.ReadAllBytes(path);

            vorbis = new StreamBuffer(command.FilePath, Vorbis.FromMemory(fileData));
        }


        track.Setup(source, vorbis);
        return track;
    }

    private void ClearCache() {
        foreach (var (key, buffer) in _staticBuffers) {
            if (buffer.ActiveCount > 0) {
                continue;
            }

            _staticBuffers.Remove(key);
        }

        foreach (var (key, buffer) in _vorbisBuffer) {
            buffer.Vorbis.Dispose();
        }
        _vorbisBuffer.Clear();
    }
}