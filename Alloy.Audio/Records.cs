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

internal interface IAudioType {
    void Play();
    void Update(double totalMs, float gain);
}

internal class Effect : IAudioType, IPoolable {
    public bool Stale { get; private set; } = false;
    private readonly int _source = AL.GenSource();
    private int _buffer;

    public void SetBuffer(int buffer) => _buffer = buffer;

    public void Play() {
        AL.Sourcei(_source, SourcePNameI.Buffer, _buffer);
        AL.SourcePlay(_source);
    }

    public void Reset() {
        Stale = false;
        _buffer = 0;
    }

    public void Update(double totalMs, float gain) {
        var state = (SourceState)AL.GetSourcei(_source, SourceGetPNameI.SourceState);
        
        if (state == SourceState.Stopped) {
            Stale = true;
            AL.Sourcei(_source, SourcePNameI.Buffer, 0);
            return;
        }
        
        AL.Sourcef(_source, SourcePNameF.Gain, gain); // Probably not ideal to set gain every update cycle but i cbf
    }
}

internal class Music : IAudioType, IPoolable {
    private const int NumBuffers = 4;
    
    public bool Stale { get; private set; } = false;
    
    private readonly int _source;
    private readonly int[] _buffers = new int[4];
    
    private Vorbis _vorbis;
    private int _bytesPerFrame;
    private Format _format;

    private double _fadeStart;
    private double _fadeDuration;
    private double _fadeEnd;
    private FadeType _fadeType;
    private double _endTime = -1;

    public Music() {
        _source = AL.GenSource();
        AL.GenBuffers(NumBuffers, _buffers);
    }

    public void SetVorbis(Vorbis vorbis) {
        _vorbis = vorbis;
        _bytesPerFrame = vorbis.Channels * sizeof(short);
        _format = InternalUtils.GetChannelFormat(vorbis.Channels);
        
        for (var i = 0; i < NumBuffers; i++) {
            _vorbis.SubmitBuffer();
            AL.BufferData(_buffers[i], _format, ref _vorbis.SongBuffer[0], _vorbis.Decoded * _bytesPerFrame, _vorbis.SampleRate);
        }
    }

    public void Play() {
        AL.SourceQueueBuffers(_source, NumBuffers, _buffers);
        AL.SourcePlay(_source);
    }
    
    public void Reset() {
        Stale = false;
        _vorbis = null;
        _endTime = -1;
    }

    public void SetFade(double start, double duration, FadeType type) {
        _fadeStart = start;
        _fadeDuration = duration;
        _fadeEnd = _fadeStart + _fadeDuration;
        _fadeType = type;
    }

    public void EndAt(double endTime) {
        _endTime = endTime;
    }

    public void Update(double totalMs, float gain) {
        if (_endTime > 0 && totalMs > _endTime) {
            AL.SourceStop(_source);
        }
        
        var state = (SourceState)AL.GetSourcei(_source, SourceGetPNameI.SourceState);

        if (state == SourceState.Stopped) {
            Stale = true;
            DequeueBuffers();
            return;
        }
        
        //TODO: improve fade control maybe
        gain *= GetFade(totalMs);
        AL.Sourcef(_source, SourcePNameF.Gain, gain);
        
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

    private float GetFade(double totalMs) {
        if (totalMs > _fadeEnd) {
            return _fadeType == FadeType.In ? 1f : 0f;
        }

        var gain = Math.Clamp((totalMs - _fadeStart) / _fadeDuration, 0d, 1d);

        if (_fadeType == FadeType.Out) {
            gain = 1 - gain;
        }

        return (float)gain;
    }

    private void DequeueBuffers() {
        AL.GetSourcei(_source, SourceGetPNameI.BuffersQueued, out var queued);

        if (queued <= 0) {
            return;
        }

        Span<int> temp = stackalloc int[queued];
        AL.SourceUnqueueBuffers(_source, queued, temp);
    }
}