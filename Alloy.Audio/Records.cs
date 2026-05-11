using System.Runtime.InteropServices;
using OpenTK.Audio.OpenAL;
using Alloy.Audio.Utils;
using StbVorbisSharp;

namespace Alloy.Audio;

[StructLayout(LayoutKind.Explicit)]
internal readonly struct EngineCommand {
    [FieldOffset(0)] public readonly AllTypes Type;
    [FieldOffset(4)] public readonly float FloatValue;
    [FieldOffset(4)] public readonly CacheType Cache;
    [FieldOffset(8)] public readonly string Path;

    public EngineCommand(AllTypes type) {
        Type = type;
    }
    
    public EngineCommand(AllTypes type, float value) {
        Type = type;
        FloatValue = value;
    }
    
    public EngineCommand(AllTypes type, string value) {
        Type = type;
        Path = value;
    }
    
    public EngineCommand(AllTypes type, float floatValue, string value) {
        Type = type;
        Path = value;
        FloatValue = floatValue;
    }
    
    public EngineCommand(AllTypes type, CacheType cache) {
        Type = type;
        Cache = cache;
    }
}

internal class Effect {
    
    private readonly int _source;
    private readonly int _buffer;
    
    public Effect(int source, int buffer) {
        _source = source;
        _buffer = buffer;
        
        AL.SourceQueueBuffer(_source, _buffer);
        AL.SourcePlay(_source);
        var state = (SourceState)AL.GetSourcei(_source, SourceGetPNameI.SourceState);
        Console.WriteLine(state);
    }
    
    public void Update(double totalMs, float gain) {
        var state = (SourceState)AL.GetSourcei(_source, SourceGetPNameI.SourceState);
        
        Console.WriteLine(state);
        
        if (state != SourceState.Playing) {
            return;
        }
        ////todo add gain and improve fade
        //HandleFade(totalMs);
        //HandleBuffers();
    }
}

internal class Music {

    private const int NumBuffers = 4;
    
    private readonly Vorbis _vorbis;
    private readonly int _source;
    private readonly int[] _buffers = new int[4];
    private readonly int _bytesPerFrame;
    private readonly Format _format;

    private double _fadeStart;
    private double _fadeDuration;
    private double _fadeEnd;
    private FadeType _fadeType;

    public Music(Vorbis song, int source) {
        _vorbis = song;
        _source = source;
        _bytesPerFrame = song.Channels * sizeof(short);
        _format = GetFormat(_vorbis);
        
        AL.GenBuffers(NumBuffers, _buffers);
        
        for (var i = 0; i < NumBuffers; i++) {
            _vorbis.SubmitBuffer();
            AL.BufferData(_buffers[i], _format, ref _vorbis.SongBuffer[0], _vorbis.Decoded * _bytesPerFrame, _vorbis.SampleRate);
        }
        
        AL.SourceQueueBuffers(_source, NumBuffers, _buffers);
        AL.SourcePlay(_source);
    }

    public void SetFade(double start, double duration, FadeType type) {
        _fadeStart = start;
        _fadeDuration = duration;
        _fadeEnd = _fadeStart + _fadeDuration;
        _fadeType = type;
    }

    public void EndAt(double endTime) {
        
    }

    public void Update(double totalMs, float gain) {
        var state = (SourceState)AL.GetSourcei(_source, SourceGetPNameI.SourceState);

        if (state != SourceState.Playing) {
            return;
        }
        //todo add gain and improve fade
        HandleFade(totalMs);
        HandleBuffers();
    }

    private void HandleBuffers() {
        AL.GetSourcei(_source, SourceGetPNameI.BuffersProcessed, out var processed);

        var index = 0;

        while (processed-- > 0) {
            AL.SourceUnqueueBuffers(_source, 1, ref _buffers[index]);

            _vorbis.SubmitBuffer();
                
            if (_vorbis.Decoded == 0) {
                _vorbis.Restart();
                _vorbis.SubmitBuffer();
            }

            AL.BufferData(_buffers[index], _format, ref _vorbis.SongBuffer[0], _vorbis.Decoded * _bytesPerFrame, _vorbis.SampleRate);
            AL.SourceQueueBuffers(_source, 1, _buffers);
            index++;
            index %= NumBuffers;
        }
    }

    private void HandleFade(double totalMs) {
        if (totalMs > _fadeEnd) {
            return;
        }

        var gain = Math.Clamp((totalMs - _fadeStart) / _fadeDuration, 0d, 1d);

        if (_fadeType == FadeType.Out) {
            gain = 1 - gain;
        }
        
        AL.Sourcef(_source, SourcePNameF.Gain, (float)gain);
        AL.Sourcef(_source, SourcePNameF.Gain, (float)gain);
    }

    private Format GetFormat(Vorbis vorbis) {
        switch (vorbis.Channels) {
            case 1: return Format.FormatMono16;
            case 2: return Format.FormatStereo16;
            default: throw new ArgumentOutOfRangeException(nameof(vorbis), vorbis.Channels, "Not mono or stereo");
        }
    }
}