using OpenTK.Audio.OpenAL;
using Alloy.Audio.Utils;
using StbVorbisSharp;

namespace Alloy.Audio;

internal class Music {

    private const int NumBuffers = 4;
    
    private readonly Vorbis _vorbis;
    private readonly int _source;
    private readonly int[] _buffers = new int[4];
    private readonly int _bytesPerFrame;

    private double _fadeStart;
    private double _fadeDuration;
    private double _fadeEnd;
    private FadeType _fadeType;

    public Music(Vorbis song, int source) {
        _vorbis = song;
        _source = source;
        _bytesPerFrame = song.Channels * sizeof(short);
        AL.GenBuffers(NumBuffers, _buffers);
        
        for (var i = 0; i < NumBuffers; i++) {
            _vorbis.SubmitBuffer();
            AL.BufferData(_buffers[i], Format.FormatStereo16, ref _vorbis.SongBuffer[0], _vorbis.Decoded * _bytesPerFrame, _vorbis.SampleRate);
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

    public void Update(double totalMs) {
        var state = (SourceState)AL.GetSourcei(_source, SourceGetPNameI.SourceState);

        if (state != SourceState.Playing) {
            return;
        }
        
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

            AL.BufferData(_buffers[index], Format.FormatStereo16, ref _vorbis.SongBuffer[0], _vorbis.Decoded * _bytesPerFrame, _vorbis.SampleRate);
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
    }
}