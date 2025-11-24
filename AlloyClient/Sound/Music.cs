using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using AlloyClient.State;
using AlloyClient.Utils;
using NAudio.Wave;

namespace AlloyClient.Sound;

public static class Music {
    public const string MusicUrl = Settings.AssetUrl + "/sfx/music/";

    private static readonly Logger Log = new(typeof(Music));

    private static readonly ConcurrentDictionary<string, Song> CachedSongs = new();
    
    private static Song _currentSong;
    
    public static void PreLoadSongs() {
        LoadMusic("Main Menu");
        LoadMusic("ULTRAKILL - The World Looks Red (Loop)");
    }
    
    public static void LoadMusic(string name) {
        if (CachedSongs.ContainsKey(name)) {
            return;
        }

        try {
            var song = new Song(name);
            CachedSongs[name] = song;
        }
        catch (Exception e) {
            Log.Error($"Failed to load song {name}: {e}");
        }
    }

    public static void PlayMusic(string name, float fadeDuration = 2f) {
        if (!Settings.PlayMusic) {
            return;
        }
        
        if (_currentSong != null) {
            if (_currentSong.Name == name) {
                return;
            }

            _currentSong.Stop();
        }

        Log.Info($"Playing song {name}");

        Task.Run(() => {
            try {
                if (!CachedSongs.ContainsKey(name)) {
                    LoadMusic(name);
                }

                _currentSong = CachedSongs[name];
                _currentSong.Play(fadeDuration);
            }
            catch (Exception e) {
                Log.Error($"Failed to play song {name}: {e}");
            }
        });
    }
    
    public static void ToggleMusic(bool state) {
        Settings.PlayMusic = state;
        
        if (state) {
            _currentSong?.Play(); // Restarts the song. Probably should be resumed
        }
        else {
            _currentSong?.Pause();
        }
    }

    public static void Stop() {
        _currentSong?.Stop();
    }
}

public class Song {
    public readonly string Name;

    private readonly WaveFormat _waveFormat;
    private readonly byte[] _audioData;
    
    private int _fadeDurationMs;

    private WaveOutEvent _outputDevice;
    private MainProvider _provider;
    
    private bool _initialized;

    public Song(string name) {
        Name = name;
        
        using var reader = new MediaFoundationReader($"{Music.MusicUrl}{name}.mp3");
        _waveFormat = reader.WaveFormat;
        _audioData = new byte[reader.Length];
        _ = reader.Read(_audioData, 0, _audioData.Length);
    }

    public void Play(float fadeDuration = 2f) {
        if (_initialized) {
            _outputDevice.Stop();
            return;
        }

        _fadeDurationMs = (int) (fadeDuration * 1000);

        var reader = new RawSourceWaveStream(_audioData, 0, _audioData.Length, _waveFormat);
        _provider = new MainProvider(reader.ToSampleProvider(), this, reader.TotalTime, _fadeDurationMs);
        _provider.BeginFadeIn();

        _outputDevice = new WaveOutEvent();
        _outputDevice.Volume = Settings.MusicVolume;
        _outputDevice.Init(_provider);
        _outputDevice.Play();
    }

    public void Stop() {
        if (_provider.State is MainProvider.FadeState.Silence or MainProvider.FadeState.FadingOut) {
            return;
        }

        if (_fadeDurationMs > 0) {
            _provider.BeginFadeOut();
        }
        else {
            DisposeDevice();
        }
    }
    
    public void Pause() {
        _outputDevice.Pause();
    }

    public void DisposeDevice() {
        _outputDevice.Dispose();
    }
}

public class MainProvider : ISampleProvider {
    public enum FadeState {
        FadingIn,
        FadingOut,
        FullVolume,
        Silence
    }

    private readonly Stopwatch _stopWatch = new();
    private readonly object _lockObject = new();

    private int _fadeSamplePosition;
    private int _fadeSampleCount;

    private readonly ISampleProvider _source;
    private readonly Song _song;
    private readonly TimeSpan _songLength;
    private readonly int _fadeDurationMs;

    public FadeState State = FadeState.FadingIn;

    public MainProvider(ISampleProvider source, Song song, TimeSpan songLength, int fadeDurationMs) {
        _source = source;
        _song = song;
        _songLength = songLength;
        _fadeDurationMs = fadeDurationMs;
        _stopWatch.Start();
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public void BeginFadeIn() {
        lock (_lockObject) {
            _fadeSamplePosition = 0;
            _fadeSampleCount = _fadeDurationMs * WaveFormat.SampleRate / 1000;
            State = FadeState.FadingIn;
        }
    }

    public void BeginFadeOut() {
        lock (_lockObject) {
            _fadeSamplePosition = 0;
            _fadeSampleCount = _fadeDurationMs * WaveFormat.SampleRate / 1000;
            State = FadeState.FadingOut;
        }
    }

    public int Read(float[] buffer, int offset, int count) {
        var sourceSamplesRead = _source.Read(buffer, offset, count);
        lock (_lockObject) {
            switch (State) {
                case FadeState.FadingIn:
                    FadeIn(buffer, offset, sourceSamplesRead);
                    break;
                case FadeState.FadingOut:
                    FadeOut(buffer, offset, sourceSamplesRead);
                    break;
                case FadeState.FullVolume:
                    if (_stopWatch.ElapsedMilliseconds >= _songLength.TotalMilliseconds - _fadeDurationMs) {
                        _song.Play();

                        BeginFadeOut();
                    }

                    break;
                case FadeState.Silence:
                    _song.DisposeDevice();
                    break;
            }
        }

        return sourceSamplesRead;
    }

    private void FadeIn(float[] buffer, int offset, int sourceSamplesRead) {
        var sample = 0;
        while (sample < sourceSamplesRead) {
            var multiplier = _fadeSamplePosition / (float) _fadeSampleCount;
            for (var ch = 0; ch < _source.WaveFormat.Channels; ch++) {
                buffer[offset + sample++] *= multiplier;
            }

            _fadeSamplePosition++;

            if (_fadeSamplePosition <= _fadeSampleCount) {
                continue;
            }

            State = FadeState.FullVolume;
            break;
        }
    }

    private void FadeOut(float[] buffer, int offset, int sourceSamplesRead) {
        var sample = 0;
        while (sample < sourceSamplesRead) {
            var multiplier = 1.0f - _fadeSamplePosition / (float) _fadeSampleCount;
            for (var ch = 0; ch < _source.WaveFormat.Channels; ch++) {
                buffer[offset + sample++] *= multiplier;
            }

            _fadeSamplePosition++;

            if (_fadeSamplePosition <= _fadeSampleCount) {
                continue;
            }

            State = FadeState.Silence;
            break;
        }
    }
}