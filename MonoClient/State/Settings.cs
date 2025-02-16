using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using MonoClient.State.Input;
using MonoClient.State.SettingTypes;
using MonoClient.Utils;

namespace MonoClient.State;

public static class Settings {
    private static readonly Logger Log = new(typeof(Settings));

    public const string SettingsPath = "settings.json"; // Relative to application directory

    public const string BuildVersion = "7.0";
    public const string BuildLabel = $"Mono v{BuildVersion}";

    //public const string AppEngineAddress = "127.0.0.1";//162.248.102.164
    public const string AppEngineAddress = "162.248.102.164";//162.248.102.164
    public const string AssetUrl = "https://domain-of-magica.github.io";

    public const string AppEnginePort = "8080";
    public const string AppEngineUrl = $"http://{AppEngineAddress}:{AppEnginePort}";

    public const int AppEngineTimeout = 10000;

    public const string RegisterCode = "Love_DoM";

    //public const string GameServerAddress = "127.0.0.1"; //162.248.102.164
    public const string GameServerAddress = "162.248.102.164"; //162.248.102.164
    public const ushort GameServerPort = 2050;

    public static float XScaleUp {
        get => (float) ScreenWidth / DefaultScreenWidth;
    }

    public static float YScaleUp {
        get => (float) ScreenHeight / DefaultScreenHeight;
    }

    public static float XScaleDown {
        get => (float) DefaultScreenWidth / ScreenWidth;
    }

    public static float YScaleDown {
        get => (float) DefaultScreenHeight / ScreenHeight;
    }

    public const int DefaultScreenWidth = 1280;
    public const int DefaultScreenHeight = 720;

    public const int ScaleFactor = 8;
    public const int MinCameraZoom = 24;
    public const int MaxCameraZoom = 400;
    
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    public static InputSetting Options = new() { Key = Keys.O };

    public static InputSetting MoveUp = new() { Key = Keys.W };
    public static InputSetting MoveDown = new() { Key = Keys.S };
    public static InputSetting MoveLeft = new() { Key = Keys.A };
    public static InputSetting MoveRight = new() { Key = Keys.D };

    public static InputSetting RotateLeft = new() { Key = Keys.Q };
    public static InputSetting RotateRight = new() { Key = Keys.E };

    public static InputSetting Shoot = new() { Mouse = MouseButton.Left };

    public static InputSetting AutoFire = new() { Key = Keys.I };

    public static InputSetting Chat = new() { Key = Keys.Enter };
    public static InputSetting ChatCommand = new() { Key = Keys.None };
    public static InputSetting TellKey = new() { Key = Keys.Tab };

    public static InputSetting Special = new() { Key = Keys.Space };

    public static InputSetting Interact = new() { Key = Keys.D0 };

    public static InputSetting Walk = new() { Key = Keys.LeftShift };

    public static InputSetting ResetCameraAngle = new() { Key = Keys.Z };

    public static InputSetting Focus = new() { Key = Keys.None };

    public static InputSetting SelectFocus = new() { Key = Keys.Tab };

    public static InputSetting PerformanceStats = new() { Key = Keys.F5 };

    public static InputSetting CenterPlayerKey = new() { Key = Keys.X };

    #region HOTKEYS

    public static InputSetting HealthPotion = new() { Key = Keys.F };
    public static InputSetting MagicPotion = new() { Key = Keys.V };

    public static InputSetting InvOne = new() { Key = Keys.D1 };
    public static InputSetting InvTwo = new() { Key = Keys.D2 };
    public static InputSetting InvThree = new() { Key = Keys.D3 };
    public static InputSetting InvFour = new() { Key = Keys.D4 };
    public static InputSetting InvFive = new() { Key = Keys.D5 };
    public static InputSetting InvSix = new() { Key = Keys.D6 };
    public static InputSetting InvSeven = new() { Key = Keys.D7 };
    public static InputSetting InvEight = new() { Key = Keys.D8 };

    public static InputSetting Escape = new() { Key = Keys.R };

    public static InputSetting MenuButton = new() { Key = Keys.Escape };

    public static InputSetting SwitchTabs = new() { Key = Keys.B };

    public static InputSetting ReconVault = new() { Key = Keys.None };
    public static InputSetting ReconGuild = new() { Key = Keys.None };
    public static InputSetting ReconRealm = new() { Key = Keys.None };
    public static InputSetting ReconGod = new() { Key = Keys.None };
    public static InputSetting ReconDarkMarket = new() { Key = Keys.None };
    public static InputSetting ReconMarket = new() { Key = Keys.None };

    public static InputSetting PartySummon = new() { Key = Keys.None };
    public static InputSetting PartyAccept = new() { Key = Keys.None };

    public static InputSetting ResetMScale = new() { Key = Keys.None };

    public static InputSetting SetBagPriority = new() { Key = Keys.None };

    public static InputSetting HidePetsKey = new() { Key = Keys.None };

    public static InputSetting FullscreenKey = new() { Key = Keys.F11 };

    public static InputSetting ItemScreenshotKey = new() { Key = Keys.F3 };
    public static InputSetting ScreenshotModeKey = new() { Key = Keys.F12 };
    public static InputSetting FreeRoamKey = new() { Key = Keys.F2 };

    #endregion

    public static InputSetting GuildChat = new() { Key = Keys.G };
    public static InputSetting PartyChat = new() { Key = Keys.P };

    public static InputSetting ChatHistoryUp = new() { Key = Keys.Up };
    public static InputSetting ChatHistoryDown = new() { Key = Keys.Down };

    public static ValueSetting<PacketLogLevel> PacketLogging = PacketLogLevel.Off;

    public static ValueSetting<bool> MovementInterpolation = true;

    public static ValueSetting<float> CameraAngle = 0f;
    public static ValueSetting<float> CameraZoom = 40;

    public static ValueSetting<float> RotateSpeed = 0.003f;

    public static ValueSetting<int> FpsCap = -1;
    public static ValueSetting<bool> VSync = false;
    public static ValueSetting<bool> Fullscreen = true;

    public static ValueSetting<int> ScreenWidth = DefaultScreenWidth;
    public static ValueSetting<int> ScreenHeight = DefaultScreenHeight;
    public static ValueSetting<int> NonFullscreenWidth = DefaultScreenWidth;
    public static ValueSetting<int> NonFullscreenHeight = DefaultScreenHeight;

    public static ValueSetting<float> MusicVolume = 0.5f;
    public static ValueSetting<float> SfxVolume = 0.5f;
    public static ValueSetting<float> WeaponSfxVolume = 0.5f;
    public static ValueSetting<float> LootSfxVolume = 0.5f;

    public static ValueSetting<bool> PlayMusic = true;
    public static ValueSetting<bool> PlaySfx = true;
    public static ValueSetting<bool> PlayWeaponSfx = true;
    public static ValueSetting<bool> PlayLootSfx = true;

    public static ValueSetting<bool> ToggleLeftToMax = true;
    public static ValueSetting<bool> ToggleBarText = true;

    public static ValueSetting<bool> AllowRotation = true;

    public static ValueSetting<bool> InventorySwap = true;

    public static ValueSetting<int> ChatInclude = 0;
    public static ValueSetting<bool> ChatVisible = true;
    public static ValueSetting<float> ChatScaling = 0f;
    public static ValueSetting<int> ChatHideList = 0;

    public static ValueSetting<bool> EyeCandyParticles = true;
    public static ValueSetting<bool> ReducedParticles = false;

    public static ValueSetting<int> MaxRenderDistance = 20;

    public static ValueSetting<float> MScale = 1;

    public static ValueSetting<bool> CenterPlayer = true;

    public static ValueSetting<ushort> SelectedGameServerPort = GameServerPort;
    
    // ReSharper restore FieldCanBeMadeReadOnly.Global

    private static readonly Dictionary<string, ISettingType> SettingTypes = [];
    public static readonly List<InputSetting> Inputs = [];

    public static void ResetToDefault() {
        PacketLogging.SetValue(PacketLogLevel.Off);

        Options.Key = Keys.O;

        MoveUp.Key = Keys.W;
        MoveDown.Key = Keys.S;
        MoveLeft.Key = Keys.A;
        MoveRight.Key = Keys.D;

        RotateLeft.Key = Keys.Q;
        RotateRight.Key = Keys.E;

        Shoot.Mouse = MouseButton.Left;

        AutoFire.Key = Keys.I;

        Chat.Key = Keys.Enter;
        ChatCommand.Key = Keys.Enter;

        Special.Key = Keys.Space;

        Interact.Key = Keys.D0;

        Escape.Key = Keys.R;

        MenuButton.Key = Keys.Escape;

        Walk.Key = Keys.LeftShift;

        HealthPotion.Key = Keys.F;
        MagicPotion.Key = Keys.V;

        InvOne.Key = Keys.D1;
        InvTwo.Key = Keys.D2;
        InvThree.Key = Keys.D3;
        InvFour.Key = Keys.D4;
        InvFive.Key = Keys.D5;
        InvSix.Key = Keys.D6;
        InvSeven.Key = Keys.D7;
        InvEight.Key = Keys.D8;

        ResetCameraAngle.Key = Keys.Z;

        CenterPlayerKey.Key = Keys.X;

        SelectFocus.Key = Keys.Tab;

        Focus.Key = Keys.LeftShift;

        PerformanceStats.Key = Keys.F5;

        SwitchTabs.Key = Keys.B;

        ReconVault.Key = Keys.None;
        ReconGuild.Key = Keys.None;
        ReconRealm.Key = Keys.None;
        ReconGod.Key = Keys.None;
        ReconDarkMarket.Key = Keys.None;
        ReconMarket.Key = Keys.None;

        PartySummon.Key = Keys.None;
        PartyAccept.Key = Keys.None;

        ResetMScale.Key = Keys.None;

        SetBagPriority.Key = Keys.None;

        HidePetsKey.Key = Keys.None;

        FullscreenKey.Key = Keys.F11;

        ItemScreenshotKey.Key = Keys.F3;
        ScreenshotModeKey.Key = Keys.F12;
        FreeRoamKey.Key = Keys.F2;

        GuildChat.Key = Keys.G;
        PartyChat.Key = Keys.P;

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
    }

    public static void LoadSettings() {
        ResetToDefault();

        try {
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
        } catch (Exception e) {
            ResetToDefault();

            Log.Error($"Error loading settings: {e.Message}");
        }

        Log.Info("Settings loaded");

        SaveSettings();
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