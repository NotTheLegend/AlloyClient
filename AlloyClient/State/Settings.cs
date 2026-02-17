using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using AlloyClient.Data;
using AlloyClient.State.SettingTypes;
using AlloyClient.Utils;
using Common;
using Newtonsoft.Json;
using OpenTK.Platform;

namespace AlloyClient.State;

public static class Settings {
    private static readonly Logger Log = new(typeof(Settings));

    public const string LocalFolderName = "AlloyClient";
    public static readonly string LocalFolderPath;
    
    private const string AccountFileName = "account.json";
    public const string SettingsPath = "settings.json"; // Relative to application directory

    public const string BuildVersion = "0.3.3";
    public const string BuildLabel = $"OpenTK v{BuildVersion}";

    public const string AppEngineAddress = "127.0.0.1";
    //public const string AppEngineAddress = "204.13.235.158";
    public const string AssetUrl = "https://domain-of-magica.github.io";

    public const string AppEnginePort = "80";
    public const string AppEngineUrl = $"http://{AppEngineAddress}:{AppEnginePort}";

    public const int AppEngineTimeout = 10000;

    public const string GameServerAddress = "127.0.0.1";
    //public const string GameServerAddress = "204.13.235.158";
    public const ushort GameServerPort = 2050;

    public const int DefaultScreenWidth = 1280;
    public const int DefaultScreenHeight = 720;

    public const float MinCameraZoom = 0.5f;
    public const float MaxCameraZoom = 5;
    
    // ReSharper disable FieldCanBeMadeReadOnly.Global
#pragma warning disable CA2211
    #region HOTKEYS
    
    public static InputSetting Options = new();

    
    // Movement
    public static InputSetting MoveUp = new();
    public static InputSetting MoveDown = new();
    public static InputSetting MoveLeft = new();
    public static InputSetting MoveRight = new();

    // Camera
    public static  InputSetting RotateLeft = new();
    public static InputSetting RotateRight = new();
    public static InputSetting ResetCameraAngle = new();
    public static InputSetting CenterPlayerKey = new();

    // Key 
    public static InputSetting AutoFire = new();
    public static InputSetting Special = new();
    public static InputSetting Interact = new();
    public static  InputSetting Escape = new();

    // Chat
    public static InputSetting Chat = new();
    public static InputSetting ChatCommand = new();
    public static InputSetting TellKey = new();
    public static InputSetting GuildChat = new();
    public static InputSetting PartyChat = new();
    public static InputSetting ChatHistoryUp = new();
    public static InputSetting ChatHistoryDown = new();

    // Inventory
    public static InputSetting HealthPotion = new();
    public static InputSetting MagicPotion = new();
    public static InputSetting InvOne = new();
    public static InputSetting InvTwo = new();
    public static InputSetting InvThree = new();
    public static InputSetting InvFour = new();
    public static InputSetting InvFive = new();
    public static InputSetting InvSix = new();
    public static InputSetting InvSeven = new();
    public static InputSetting InvEight = new();
    
    // Misc
    public static InputSetting PerformanceStats = new();
    public static InputSetting SwitchTabs = new();
    public static InputSetting ResetMScale = new();
    public static InputSetting SetBagPriority = new();
    public static InputSetting FullscreenKey = new();
    
    #endregion

    #region VALUES

    public static ValueSetting<PacketLogLevel> PacketLogging = PacketLogLevel.Off;

    public static ValueSetting<bool> MovementInterpolation = true;

    public static ValueSetting<float> CameraAngle = 0f;
    public static ValueSetting<float> CameraZoom = 1;

    public static ValueSetting<float> RotateSpeed = 0.003f;

    public static ValueSetting<int> FpsCap = -1;
    public static ValueSetting<bool> VSync = false;
    public static ValueSetting<bool> Fullscreen = false;

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
    
    #endregion
    
    // ReSharper restore FieldCanBeMadeReadOnly.Global
#pragma warning restore CA2211

    private static readonly Dictionary<string, ISettingType> SettingTypes = [];
    public static readonly List<InputSetting> Inputs = [];

    static Settings() {
        LocalFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), LocalFolderName);
        Directory.CreateDirectory(LocalFolderPath);
    }

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
        CameraZoom.SetValue(1);

        RotateSpeed.SetValue(0.003f);

        FpsCap.SetValue(-1);
        VSync.SetValue(false);
        Fullscreen.SetValue(false);

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
        LoadLocalAccount();
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
    
    public static void LoadLocalAccount() {
        var filePath = Path.Combine(LocalFolderPath, AccountFileName);

        if (!File.Exists(filePath)) {
            Log.Debug("No local account data found");
            return;
        }

        string text;
        try {
            text = File.ReadAllText(filePath);
        } catch (Exception e) {
            Log.Error($"Failed to read to file {AccountFileName}: {e.Message}");
            return;
        }
        
        var info = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

        if (info == null) {
            Log.Debug("Failed to parse local account data");
            return;
        }

        var loadedUser = info.TryGetValue("username", out var username) && !string.IsNullOrWhiteSpace(username);
        var loadedPass = info.TryGetValue("password", out var password) && !string.IsNullOrWhiteSpace(password);

        if (username == string.Empty && password == string.Empty) {
            Log.Debug("No local account data");
            return;
        }

        if (!loadedUser || !loadedPass) {
            Log.Debug("Incomplete/Invalid local account data");
            return;
        }

        var data = new byte[(password.Length * 3 + 3) / 4];
        
        if (!Convert.TryFromBase64String(password, data.AsSpan(), out var count)) {
            Log.Error("Invalid Base64 encoding on password");
            return;
        }

        GlobalData.Add(new LoginData(username, Encoding.UTF8.GetString(data.AsSpan(0, count))));
    }

    public static void SaveLocalAccount() {
        var data = GlobalData.Get<LoginData>() ?? LoginData.Default;
        
        var bytes = new byte[Encoding.UTF8.GetByteCount(data.Password.AsSpan())];
        if (!Encoding.UTF8.TryGetBytes(data.Password.AsSpan(), bytes.AsSpan(), out var byteCount)) {
            Log.Error("Failed to get password bytes");
            return;
        }

        var chars = new char[4 * (data.Password.Length + 2) / 3];
        if (!Convert.TryToBase64Chars(bytes.AsSpan(0, byteCount), chars.AsSpan(), out var charCount)) {
            Log.Error("Failed to Base64 encode password");
            return;
        }

        var username = data.Username;
        var password = new string(chars.AsSpan(0, charCount));

        var jsonString = JsonConvert.SerializeObject(new Dictionary<string, string>{{"username", username}, {"password", password}});

        try {
            File.WriteAllText(Path.Combine(LocalFolderPath, AccountFileName), jsonString);
        } catch (Exception e) {
            Log.Error($"Failed to write to file {AccountFileName}: {e.Message}");
        }
    }
}