using Alloy.Audio.Utils;
using OpenTK.Audio.OpenAL;

namespace Alloy.Audio;

internal abstract class Track : IPoolable {
    
    protected int Source;
    
    protected double StopTime;
    protected float Volume = 1f;
    protected float LastVolume = -1f;

    private AudioFade _fadeState = AudioFade.In;
    private double _fadeStart = -1d;
    private double _fadeDuration = -1d;
    
    public void Play() => AL.SourcePlay(Source);

    public abstract bool Update(double time, float channelVolume);

    public void Stop(double time) => StopTime = time;

    public void SetFadeState(FadeState state, double time) {
        _fadeState = state.Fade;
        _fadeDuration = state.DurationMs;
        _fadeStart = time;
    }

    protected float GetFadeGain(double time) {
        if (_fadeStart <= 0d || _fadeDuration <= 0d) {
            return _fadeState == AudioFade.In ? 1f : 0f;
        }
        
        var gain = (time - _fadeStart) / _fadeDuration;

        if (gain < 0d || gain > 1f) {
            gain = Math.Clamp(gain, 0d, 1d);
            _fadeStart = _fadeDuration = -1d;
        }

        if (_fadeState == AudioFade.Out) {
            gain = 1 - gain;
        }

        return (float)gain;
    }
    
    public void SetVolume(float volume) => Volume = volume;

    public abstract void SetAudioState(AudioState state);

    public virtual void Clear() {
        StopTime = -1d;
        Volume = 1f;
        LastVolume = -1f;
        _fadeState = AudioFade.In;
        _fadeStart = _fadeDuration = -1d;
    }
}

internal class StaticTrack : Track {

    private StaticBuffer _buffer;

    public StaticTrack() : base() { }
    
    public void Setup(int source, StaticBuffer buffer) {
        Source = source;
        _buffer = buffer;
        _buffer.ActiveCount++;
        AL.Sourcei(Source, SourcePNameI.Buffer, _buffer.Id);
    }

    public override bool Update(double time, float channelVolume) {
        if (StopTime > 0 && time > StopTime) {
            return false;
        }

        var volume = Volume * channelVolume * GetFadeGain(time);
        if (!volume.Equals(LastVolume)) {
            LastVolume = volume;
            AL.Sourcef(Source, SourcePNameF.Gain, volume);
        }
        
        var state = (SourceState)AL.GetSourcei(Source, SourceGetPNameI.SourceState);
        if (state == SourceState.Stopped) {
            return false;
        }
        
        return true;
    }

    public override void SetAudioState(AudioState state) => AL.Sourcei(Source, SourcePNameI.Looping, (int)state);
    
    public override void Clear() {
        AL.SourceStop(Source);
        AL.Sourcei(Source, SourcePNameI.Buffer, 0);
        AL.Sourcei(Source, SourcePNameI.Looping, 0);
        
        _buffer.ActiveCount--;
        _buffer = null;
        
        Pools.Sources.Push(Source);
        base.Clear();
    }
}

internal class StreamTrack : Track {
    
    private const int NumBuffers = 4;
    private readonly int[] _buffers = new int[4];
    
    private StreamBuffer _stream;
    private int _bytesPerFrame;
    private Format _format;
    private bool _loop;

    public StreamTrack() {
        AL.GenBuffers(4, _buffers);
    }
    
    public void Setup(int source, StreamBuffer stream) {
        Source = source;
        _stream = stream;
        
        _bytesPerFrame = _stream.Vorbis.Channels * sizeof(short);
        _format = InternalUtils.GetChannelFormat(_stream.Vorbis.Channels);
        
        for (var i = 0; i < NumBuffers; i++) { // TODO: should probably check for data so we dont buffer 0 bytes
            _stream.Vorbis.SubmitBuffer();
            AL.BufferData(_buffers[i], _format, ref _stream.Vorbis.SongBuffer[0], _stream.Vorbis.Decoded * _bytesPerFrame, _stream.Vorbis.SampleRate);
        }
        
        AL.SourceQueueBuffers(Source, NumBuffers, _buffers);
    }
    
    public override bool Update(double time, float channelVolume) {
        if (StopTime > 0 && time > StopTime) {
            return false;
        }
        
        var volume = Volume * channelVolume * GetFadeGain(time);
        if (!volume.Equals(LastVolume)) {
            LastVolume = volume;
            AL.Sourcef(Source, SourcePNameF.Gain, volume);
        }
        
        var state = (SourceState)AL.GetSourcei(Source, SourceGetPNameI.SourceState);
        if (state == SourceState.Stopped) {
            return false;
        }
        
        FillBuffers();
        
        return true;
    }
    
    public override void SetAudioState(AudioState state) => _loop = state == AudioState.Loop;
    
    public override void Clear() {
        AL.SourceStop(Source);
        AL.SourceUnqueueBuffers(Source, NumBuffers, _buffers);
        
        Pools.Sources.Push(Source);
        base.Clear();
    }

    public StreamBuffer ClearVorbis() {
        var buffer = _stream;
        _stream = null;
        return buffer;
    }
    
    private void FillBuffers() {
        AL.GetSourcei(Source, SourceGetPNameI.BuffersProcessed, out var processed);

        var index = 0;

        while (processed-- > 0) {
            AL.SourceUnqueueBuffers(Source, 1, ref _buffers[index]);

            _stream.Vorbis.SubmitBuffer();
                
            if (_stream.Vorbis.Decoded == 0 && _loop) {
                _stream.Vorbis.Restart();
                _stream.Vorbis.SubmitBuffer();
            }

            AL.BufferData(_buffers[index], _format, ref _stream.Vorbis.SongBuffer[0], _stream.Vorbis.Decoded * _bytesPerFrame, _stream.Vorbis.SampleRate);
            AL.SourceQueueBuffers(Source, 1, ref _buffers[index]);
            index++;
            index %= NumBuffers;
        }
    }
}