using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using MonoClient.State.SettingTypes;
using MonoClient.Utils;
using OpenTK.Platform;

namespace MonoClient.State;

public static class Settings {
    private static readonly Logger Log = new(typeof(Settings));

    public const string SettingsPath = "settings.json"; // Relative to application directory

    public const string BuildVersion = "7.0";
    public const string BuildLabel = $"OpenTK v{BuildVersion}";

    //public const string AppEngineAddress = "127.0.0.1";
    public const string AppEngineAddress = "204.13.235.158";
    public const string AssetUrl = "https://domain-of-magica.github.io";

    public const string AppEnginePort = "8080";
    public const string AppEngineUrl = $"http://{AppEngineAddress}:{AppEnginePort}";

    public const int AppEngineTimeout = 10000;

    //public const string GameServerAddress = "127.0.0.1";
    public const string GameServerAddress = "204.13.235.158";
    public const ushort GameServerPort = 2050;

    public const int DefaultScreenWidth = 1280;
    public const int DefaultScreenHeight = 720;

    public const int ScaleFactor = 8;
    public const int MinCameraZoom = 24;
    public const int MaxCameraZoom = 400;
    
    #region HOTKEYS
    
    public static readonly InputSetting Options = new();
    
    // Movement
    public static readonly InputSetting MoveUp = new();
    public static readonly InputSetting MoveDown = new();
    public static readonly InputSetting MoveLeft = new();
    public static readonly InputSetting MoveRight = new();

    // Camera
    public static readonly InputSetting RotateLeft = new();
    public static readonly InputSetting RotateRight = new();
    public static readonly InputSetting ResetCameraAngle = new();
    public static readonly InputSetting CenterPlayerKey = new();

    // Key 
    public static readonly InputSetting AutoFire = new();
    public static readonly InputSetting Special = new();
    public static readonly InputSetting Interact = new();
    public static readonly InputSetting Escape = new();

    // Chat
    public static readonly InputSetting Chat = new();
    public static readonly InputSetting ChatCommand = new();
    public static readonly InputSetting TellKey = new();
    public static readonly InputSetting GuildChat = new();
    public static readonly InputSetting PartyChat = new();
    public static readonly InputSetting ChatHistoryUp = new();
    public static readonly InputSetting ChatHistoryDown = new();

    // Inventory
    public static readonly InputSetting HealthPotion = new();
    public static readonly InputSetting MagicPotion = new();
    public static readonly InputSetting InvOne = new();
    public static readonly InputSetting InvTwo = new();
    public static readonly InputSetting InvThree = new();
    public static readonly InputSetting InvFour = new();
    public static readonly InputSetting InvFive = new();
    public static readonly InputSetting InvSix = new();
    public static readonly InputSetting InvSeven = new();
    public static readonly InputSetting InvEight = new();
    
    // Misc
    public static readonly InputSetting PerformanceStats = new();
    public static readonly InputSetting SwitchTabs = new();
    public static readonly InputSetting ResetMScale = new();
    public static readonly InputSetting SetBagPriority = new();
    public static readonly InputSetting FullscreenKey = new();
    
    #endregion

    #region VALUES

    public static readonly ValueSetting<PacketLogLevel> PacketLogging = PacketLogLevel.Off;

    public static readonly ValueSetting<bool> MovementInterpolation = true;

    public static ValueSetting<float> CameraAngle = 0f;
    public static ValueSetting<float> CameraZoom = 40;

    public static readonly ValueSetting<float> RotateSpeed = 0.003f;

    public static readonly ValueSetting<int> FpsCap = -1;
    public static readonly ValueSetting<bool> VSync = false;
    public static ValueSetting<bool> Fullscreen = true;

    public static ValueSetting<int> ScreenWidth = DefaultScreenWidth;
    public static ValueSetting<int> ScreenHeight = DefaultScreenHeight;
    public static ValueSetting<int> NonFullscreenWidth = DefaultScreenWidth;
    public static ValueSetting<int> NonFullscreenHeight = DefaultScreenHeight;

    public static readonly ValueSetting<float> MusicVolume = 0.5f;
    public static readonly ValueSetting<float> SfxVolume = 0.5f;
    public static readonly ValueSetting<float> WeaponSfxVolume = 0.5f;
    public static readonly ValueSetting<float> LootSfxVolume = 0.5f;

    public static ValueSetting<bool> PlayMusic = true;
    public static readonly ValueSetting<bool> PlaySfx = true;
    public static readonly ValueSetting<bool> PlayWeaponSfx = true;
    public static readonly ValueSetting<bool> PlayLootSfx = true;

    public static readonly ValueSetting<bool> ToggleLeftToMax = true;
    public static readonly ValueSetting<bool> ToggleBarText = true;

    public static readonly ValueSetting<bool> AllowRotation = true;

    public static readonly ValueSetting<bool> InventorySwap = true;

    public static readonly ValueSetting<int> ChatInclude = 0;
    public static readonly ValueSetting<bool> ChatVisible = true;
    public static readonly ValueSetting<float> ChatScaling = 0f;
    public static readonly ValueSetting<int> ChatHideList = 0;

    public static readonly ValueSetting<bool> EyeCandyParticles = true;
    public static readonly ValueSetting<bool> ReducedParticles = false;

    public static readonly ValueSetting<int> MaxRenderDistance = 20;

    public static readonly ValueSetting<float> MScale = 1;

    public static readonly ValueSetting<bool> CenterPlayer = true;

    public static readonly ValueSetting<ushort> SelectedGameServerPort = GameServerPort;
    
    #endregion

    private static readonly Dictionary<string, ISettingType> SettingTypes = [];
    public static readonly List<InputSetting> Inputs = [];

    public static void ResetToDefault() {
        PacketLogging.SetValue(PacketLogLevel.Off);
        Options.Set(Scancode.Escape);

        #region  HOTKEYS

        // Movement
        MoveUp.Set(Scancode.W);
        MoveDown.Set(Scancode.S);
        MoveLeft.Set(Scancode.A);
        MoveRight.Set(Scancode.D);

        // Camera
        RotateLeft.Set(Scancode.Q);
        RotateRight.Set(Scancode.E);
        ResetCameraAngle.Set(Scancode.Z);
        CenterPlayerKey.Set(Scancode.X);

        // Key 
        AutoFire.Set(Scancode.I);
        Special.Set(Scancode.Spacebar);
        Interact.Set(Scancode.D0);
        Escape.Set(Scancode.R);

        // Chat
        Chat.Set(Scancode.Return);
        ChatCommand.Set(Scancode.QuestionMark);
        TellKey.Set(Scancode.Tab);
        GuildChat.Set(Scancode.G);
        PartyChat.Set(Scancode.P);
        ChatHistoryUp.Set(Scancode.PageUp);
        ChatHistoryDown.Set(Scancode.PageDown);

        // Inventory
        HealthPotion.Set(Scancode.F);
        MagicPotion.Set(Scancode.V);
        InvOne.Set(Scancode.D1);
        InvTwo.Set(Scancode.D2);
        InvThree.Set(Scancode.D3);
        InvFour.Set(Scancode.D4);
        InvFive.Set(Scancode.D5);
        InvSix.Set(Scancode.D6);
        InvSeven.Set(Scancode.D7);
        InvEight.Set(Scancode.D8);

        // Misc
        PerformanceStats.Set(Scancode.F5);
        SwitchTabs.Set(Scancode.B);
        ResetMScale.Set(Scancode.Unknown);
        SetBagPriority.Set(Scancode.Unknown);
        FullscreenKey.Set(Scancode.F11);

        #endregion

        #region VALUES

        MovementInterpolation.SetValue(true);

        CameraAngle.SetValue(0f);
        CameraZoom.SetValue(40);

        RotateSpeed.SetValue(0.003f);

        FpsCap.SetValue(-1);
        VSync.SetValue(false);
        Fullscreen.SetValue(true);

        MusicVolume.SetValue(0.5f);
        SfxVolume.SetValue(0.5f);
        WeaponSfxVolume.SetValue(0.5f);
        LootSfxVolume.SetValue(0.5f);

        PlayMusic.SetValue(true);
        PlaySfx.SetValue(true);
        PlayWeaponSfx.SetValue(true);
        PlayLootSfx.SetValue(true);

        ToggleLeftToMax.SetValue(true);
        ToggleBarText.SetValue(true);

        AllowRotation.SetValue(true);

        InventorySwap.SetValue(true);

        ChatInclude.SetValue(0);
        ChatVisible.SetValue(true);
        ChatScaling.SetValue(0f);
        ChatHideList.SetValue(0);

        EyeCandyParticles.SetValue(true);
        ReducedParticles.SetValue(false);

        MaxRenderDistance.SetValue(20);

        MScale.SetValue(1f);

        CenterPlayer.SetValue(true);

        SelectedGameServerPort.SetValue(GameServerPort);
        
        #endregion
    }

    public static void LoadSettings() {
        try {
            ResetToDefault();
            TryLoadSettings();
        } catch (Exception e) {
            Log.Error($"Error loading settings: {e.Message}");
            ResetToDefault();
        }
        
        SaveSettings();
    }

    private static void TryLoadSettings() {
        if (!File.Exists(SettingsPath)) {
            Log.Info("Settings file not found.");
            return;
        }

        var settingsFile = File.ReadAllText(SettingsPath);
        var settingsXml = new XmlDocument();
        settingsXml.LoadXml(settingsFile);

        var settingsRoot = settingsXml.DocumentElement;
        if (settingsRoot == null) {
            Log.Warn("Settings file is empty.");
            SaveSettings();
            return;
        }

        foreach (var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static)) {
            if (!typeof(ISettingType).IsAssignableFrom(field.FieldType)) {
                continue;
            }

            var settingElem = settingsRoot[field.Name];
            if (settingElem == null) {
                Log.Warn($"Setting not found: {field.Name}");
                continue;
            }

            var setting = (ISettingType) field.GetValue(null);
            if (setting == null) {
                Log.Warn($"Failed to get setting: {field.Name}");
                continue;
            }

            setting.Deserialize(settingElem.InnerText);
            field.SetValue(null, setting);

            SettingTypes[field.Name] = setting;
            if (setting is InputSetting inputSetting) {
                Inputs.Add(inputSetting);
            }
        }

        Log.Info("Settings loaded");
    }

    public static void SaveSettings() {
        try {
            var settingsDict = new Dictionary<string, string>();
            foreach (var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static)) {
                if (!typeof(ISettingType).IsAssignableFrom(field.FieldType)) {
                    continue;
                }

                var setting = (ISettingType) field.GetValue(null);

                if (setting == null) {
                    Log.Warn($"Failed to get setting: {field.Name}");
                    continue;
                }

                settingsDict[field.Name] = setting.Serialize();
            }

            var settingsXml = new XmlDocument();
            var settingsRoot = settingsXml.CreateElement("Settings");
            settingsXml.AppendChild(settingsRoot);

            foreach (var (name, value) in settingsDict) {
                var settingElem = settingsXml.CreateElement(name);
                settingElem.InnerText = value;
                settingsRoot.AppendChild(settingElem);
            }

            settingsXml.Save(SettingsPath);

            Log.Info("Settings saved.");
        } catch (Exception e) {
            Log.Warn($"Failed to save settings: {e}");
        }
    }

    public static void SetWindowSize(int width, int height) {
        ScreenWidth = width;
        ScreenHeight = height;
    }

    public static T GetSetting<T>(string settingKey) where T : ISettingType {
        if (!SettingTypes.TryGetValue(settingKey, out var setting)) {
            return default;
        }

        return (T) setting;
    }

    public static void SetSetting(string settingKey, ISettingType value) {
        if (!SettingTypes.TryGetValue(settingKey, out var setting)) {
            Logger.Error($"Failed to save, setting not found: {settingKey}");
            return;
        }

        setting.SetValue(value); // Makes sure we're updating the static field, no use of reflection :pray:
    }
}