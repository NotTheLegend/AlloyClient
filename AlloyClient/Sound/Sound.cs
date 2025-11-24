using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using AlloyClient.State;
using AlloyClient.Utils;
using NAudio.Wave;

namespace AlloyClient.Sound;

public enum SoundType {
    Sfx,
    Weapon,
    Loot
}

public static class SoundManager {
    private static readonly ConcurrentDictionary<string, SoundEffect> CachedSoundEffects = new();

    public static void PreLoadSounds() {
        LoadSoundEffect("button_click");
        LoadSoundEffect("death_screen");
        LoadSoundEffect("enter_realm");
        LoadSoundEffect("error");
        LoadSoundEffect("inventory_move_item");
        LoadSoundEffect("level_up");
        LoadSoundEffect("loot_appears");
        LoadSoundEffect("no_mana");
        LoadSoundEffect("use_key");
        LoadSoundEffect("use_potion");
        
        LoadSoundEffect("loot/legendary_loot");
        LoadSoundEffect("loot/demonic_loot");
        LoadSoundEffect("loot/cosmic_loot");
        LoadSoundEffect("loot/angelic_loot");
    }

    public static void PlaySoundEffect(string name, SoundType soundType = SoundType.Sfx) {
        var volume = 1f;
        switch (soundType) {
            case SoundType.Sfx:
                if (!Settings.PlaySfx) {
                    return;
                }

                volume = Settings.SfxVolume;
                break;
            case SoundType.Weapon:
                if (!Settings.PlayWeaponSfx) {
                    return;
                }

                volume = Settings.WeaponSfxVolume;
                break;
            case SoundType.Loot:
                if (!Settings.PlayLootSfx) {
                    return;
                }

                volume = Settings.LootSfxVolume;
                break;
        }

        LoadSoundEffect(name);
        CachedSoundEffects[name].Play(volume);
    }
    
    private static void LoadSoundEffect(string name) {
        if (CachedSoundEffects.ContainsKey(name)) {
            return;
        }

        CachedSoundEffects[name] = new SoundEffect($"{Settings.AssetUrl}/sfx/{name}.mp3");
    }
}

public class SoundEffect {
    private static readonly Logger Log = new(typeof(SoundEffect));
    
    private readonly Queue<WaveOutEvent> _soundEffectDevices = new();
    private readonly WaveFormat _waveFormat;
    private readonly byte[] _audioData;

    public SoundEffect(string url) {
        using var reader = new MediaFoundationReader(url);
        _waveFormat = reader.WaveFormat;
        _audioData = new byte[reader.Length];
        _ = reader.Read(_audioData, 0, _audioData.Length);
    }

    public void Play(float volume = 1f) {
        if (_soundEffectDevices.Count > 0) {
            var soundEffectInstance = _soundEffectDevices.Dequeue();
            soundEffectInstance.Volume = volume;
            soundEffectInstance.Play();
            return;
        }

        var reader = new RawSourceWaveStream(_audioData, 0, _audioData.Length, _waveFormat);
        var newInstance = new WaveOutEvent();
        newInstance.Volume = volume;
        newInstance.Init(reader);
        newInstance.PlaybackStopped += (_, _) => {
            _soundEffectDevices.Enqueue(newInstance);
            reader.Seek(0, SeekOrigin.Begin);
        };
        newInstance.Play();
    }
}