using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using AlloyClient.Data;
using AlloyClient.Utils;
using Alloy.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient;

public static class Settings {
    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(Settings));

    private const string LocalFolderName = "AlloyClient";
    private const string AccountFileName = "account.json"; // these should be .xml but its funnier if they're not
    private const string SettingsFileName = "settings.json";

    private static readonly string AccountFilePath;
    private static readonly string SettingsFilePath;

    public const string BuildVersion = "0.3.3";
    public const string BuildLabel = $"Alloy v{BuildVersion}";

    public const string AppEngineAddress = "127.0.0.1";
    public const string AppEnginePort = "8080";
    public const string AppEngineUrl = $"http://{AppEngineAddress}:{AppEnginePort}";

    public const int AppEngineTimeout = 10000;

    public const string GameServerAddress = "127.0.0.1";
    public const ushort GameServerPort = 2050;

    public const int DefaultScreenWidth = 1280;
    public const int DefaultScreenHeight = 720;

    public const float MinCameraZoom = 0.5f;
    public const float MaxCameraZoom = 5;
    
    #region HOTKEYS
    
    // Movement
    public static readonly InputSetting MoveUp = new(Scancode.W);
    public static readonly InputSetting MoveDown = new(Scancode.S);
    public static readonly InputSetting MoveLeft = new(Scancode.A);
    public static readonly InputSetting MoveRight = new(Scancode.D);

    // Camera
    public static readonly InputSetting RotateLeft = new(Scancode.Q);
    public static readonly InputSetting RotateRight = new(Scancode.E);
    public static readonly InputSetting ResetCameraAngle = new(Scancode.Z);
    public static readonly InputSetting CenterPlayerKey = new(Scancode.X);

    // Key
    public static readonly InputSetting Options = new(Scancode.Escape);
    public static readonly InputSetting AutoFire = new(Scancode.I);
    public static readonly InputSetting Special = new(Scancode.Spacebar);
    public static readonly InputSetting Interact = new(Scancode.D0);
    public static readonly InputSetting Escape = new(Scancode.R);

    // Chat
    public static readonly InputSetting Chat = new(Scancode.Return);
    public static readonly InputSetting ChatCommand = new(Scancode.QuestionMark);
    public static readonly InputSetting TellKey = new(Scancode.Tab);
    public static readonly InputSetting GuildChat = new(Scancode.G);
    public static readonly InputSetting PartyChat = new(Scancode.P);
    public static readonly InputSetting ChatHistoryUp = new(Scancode.PageUp);
    public static readonly InputSetting ChatHistoryDown = new(Scancode.PageDown);

    // Inventory
    public static readonly InputSetting HealthPotion = new(Scancode.F);
    public static readonly InputSetting MagicPotion = new(Scancode.V);
    public static readonly InputSetting InvOne = new(Scancode.D1);
    public static readonly InputSetting InvTwo = new(Scancode.D2);
    public static readonly InputSetting InvThree = new(Scancode.D3);
    public static readonly InputSetting InvFour = new(Scancode.D4);
    public static readonly InputSetting InvFive = new(Scancode.D5);
    public static readonly InputSetting InvSix = new(Scancode.D6);
    public static readonly InputSetting InvSeven = new(Scancode.D7);
    public static readonly InputSetting InvEight = new(Scancode.D8);
    
    // Misc
    public static readonly InputSetting PerformanceStats = new(Scancode.F5);
    public static readonly InputSetting SwitchTabs = new(Scancode.B);
    public static readonly InputSetting ResetMScale = new(Scancode.Unknown);
    public static readonly InputSetting SetBagPriority = new(Scancode.Unknown);
    public static readonly InputSetting FullscreenKey = new(Scancode.F11);
    
    #endregion
    
    #region VALUES

    // Random
    public static readonly ValueSetting<PacketLogLevel> PacketLogging = new(PacketLogLevel.Off);
    public static readonly ValueSetting<ushort> SelectedGameServerPort = new(GameServerPort);
    
    // Camera
    public static readonly ValueSetting<int> MaxRenderDistance = new(20);
    public static readonly ValueSetting<bool> CenterPlayer = new(true);
    public static readonly ValueSetting<float> CameraAngle = new(0f);
    public static readonly ValueSetting<float> CameraZoom = new(1f);
    public static readonly ValueSetting<bool> AllowRotation = new(true);
    public static readonly ValueSetting<float> RotateSpeed = new(0.003f);

    // Screen
    public static readonly ValueSetting<int> FpsCap = new(-1);
    public static readonly ValueSetting<bool> VSync = new(false);
    public static readonly ValueSetting<WindowMode> LastWindowMode = new(WindowMode.Normal);
    public static readonly ValueSetting<int> LastWindowPositionX = new(0);
    public static readonly ValueSetting<int> LastWindowPositionY = new(0);
    public static readonly ValueSetting<FullscreenType> FullscreenMode = new(FullscreenType.Borderless);
    public static readonly ValueSetting<bool> FullscreenState = new(false);
    public static readonly ValueSetting<int> ScreenWidth = new(DefaultScreenWidth);
    public static readonly ValueSetting<int> ScreenHeight = new(DefaultScreenHeight);

    // Audio
    public static readonly ValueSetting<float> MasterVolume = new(0.5f);
    public static readonly ValueSetting<float> MusicVolume = new(1f);
    public static readonly ValueSetting<float> SfxVolume = new(1f);
    public static readonly ValueSetting<bool> PlayMaster = new(true);
    public static readonly ValueSetting<bool> PlayMusic = new(true);
    public static readonly ValueSetting<bool> PlaySfx = new(true);
    
    // Chat
    public static readonly ValueSetting<int> ChatInclude = new(0);
    public static readonly ValueSetting<bool> ChatVisible = new(true);
    public static readonly ValueSetting<float> ChatScaling = new(0f);
    public static readonly ValueSetting<int> ChatHideList = new(0);

    // Particles
    public static readonly ValueSetting<bool> EyeCandyParticles = new(true);
    public static readonly ValueSetting<bool> ReducedParticles = new(false);

    // Other
    public static readonly ValueSetting<bool> ToggleLeftToMax = new(true);
    public static readonly ValueSetting<bool> ToggleBarText = new(true);
    public static readonly ValueSetting<bool> InventorySwap = new(true);
    public static readonly ValueSetting<bool> MovementInterpolation = new(true);
    
    #endregion
    
    public static readonly InputSetting[] Inputs;

    private static readonly ReadOnlyDictionary<string, ISettingType> SettingsLookup;

    static Settings() {
        var localFolderPath = Path.CombineAlt(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), LocalFolderName);
        Directory.CreateDirectory(localFolderPath);

        AccountFilePath = Path.CombineAlt(localFolderPath, AccountFileName);
        SettingsFilePath = Path.CombineAlt(localFolderPath, SettingsFileName);

        SettingsLookup = typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => typeof(ISettingType).IsAssignableFrom(field.FieldType))
            .Select(field => (field.Name, (ISettingType) field.GetValue(null)))
            .ToDictionary().AsReadOnly();

        Inputs = SettingsLookup.Select(pair => pair.Value).OfType<InputSetting>().ToArray();
    }

    public static void ResetToDefault() {
        foreach (var (key, setting) in SettingsLookup) {
            setting.ResetToDefault();
        }
    }

    public static float GetMasterVolume() => PlayMaster ? MasterVolume : 0;
    
    public static float GetMusicVolume() => PlayMusic ? MusicVolume : 0;

    public static float GetSfxVolume() => PlaySfx ? SfxVolume : 0;
    
    #region SettingParsing
    
    public static void LoadSettings() {
        LoadLocalAccount();
        try {
            TryLoadSettings();
        } catch (Exception e) {
            Logger.Log(LogLevel.Error, $"Error loading settings: {e.Message}");
        }
        SaveSettings();
    }
    
    private static void TryLoadSettings() {
        if (!File.Exists(SettingsFilePath)) {
            Logger.Log(LogLevel.Information, "Settings file not found.");
            return;
        }
        
        var settingsXml = new XmlDocument();
        settingsXml.LoadXml(File.ReadAllText(SettingsFilePath));

        var settingsRoot = settingsXml.DocumentElement;
        if (settingsRoot == null) {
            Logger.Log(LogLevel.Warning, "Settings file is empty.");
            return;
        }
        
        var count = 0;
        foreach (var (key, setting) in SettingsLookup) {
            var tag = settingsRoot[key];

            if (tag == null) {
                continue;
            }
            
            try {
                setting.Deserialize(tag.InnerText);
                count++;
            } catch (Exception e) {
                Logger.Log(LogLevel.Warning, $"Error loading setting {key}: {e.Message}");
            }
        }
        
        Logger.Log(LogLevel.Information, $"Loaded {count} of {SettingsLookup.Count} settings, {SettingsLookup.Count - count} reset to default");
    }

    public static void SaveSettings() {
        var xml = new XmlDocument();
        var root = xml.CreateElement("Settings");
        xml.AppendChild(root);

        var count = 0;
        foreach (var (key, setting) in SettingsLookup) {
            var tag = xml.CreateElement(key);
            try {
                tag.InnerText = setting.Serialize();
            } catch (Exception e) {
                Logger.Log(LogLevel.Warning, $"Error saving setting {key}: {e.Message}");
                continue;
            }
            root.AppendChild(tag);
            count++;
        }
        
        try {
            xml.Save(SettingsFilePath);
            Logger.Log(LogLevel.Information, $"Saved {count} of {SettingsLookup.Count} settings");
        } catch (Exception e) {
            Logger.Log(LogLevel.Error, $"Failed to save settings: {e}");
        }
    }
    
    #endregion
    
    #region LocalAccountParsing
    
    public static void LoadLocalAccount() {
        if (!File.Exists(AccountFilePath)) {
            Logger.Log(LogLevel.Debug, "No local account data found");
            return;
        }

        string text;
        try {
            text = File.ReadAllText(AccountFilePath);
        } catch (Exception e) {
            Logger.Log(LogLevel.Error, $"Failed to read to file {AccountFilePath}: {e.Message}");
            return;
        }
        
        var info = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

        if (info == null) {
            Logger.Log(LogLevel.Debug, "Failed to parse local account data");
            return;
        }

        var loadedUser = info.TryGetValue("username", out var username) && !string.IsNullOrWhiteSpace(username);
        var loadedPass = info.TryGetValue("password", out var password) && !string.IsNullOrWhiteSpace(password);

        if (username == string.Empty && password == string.Empty) {
            Logger.Log(LogLevel.Debug, "No local account data");
            return;
        }

        if (!loadedUser || !loadedPass) {
            Logger.Log(LogLevel.Debug, "Incomplete/Invalid local account data");
            return;
        }

        var data = new byte[(password.Length * 3 + 3) / 4];
        
        if (!Convert.TryFromBase64String(password, data.AsSpan(), out var count)) {
            Logger.Log(LogLevel.Error, "Invalid Base64 encoding on password");
            return;
        }

        GlobalData.Add(new LoginData(username, Encoding.UTF8.GetString(data.AsSpan(0, count))));
    }

    public static void SaveLocalAccount() {
        var data = GlobalData.Get<LoginData>() ?? LoginData.Default;
        
        var bytes = new byte[Encoding.UTF8.GetByteCount(data.Password.AsSpan())];
        if (!Encoding.UTF8.TryGetBytes(data.Password.AsSpan(), bytes.AsSpan(), out var byteCount)) {
            Logger.Log(LogLevel.Error, "Failed to get password bytes");
            return;
        }

        var chars = new char[4 * (data.Password.Length + 2) / 3];
        if (!Convert.TryToBase64Chars(bytes.AsSpan(0, byteCount), chars.AsSpan(), out var charCount)) {
            Logger.Log(LogLevel.Error, "Failed to Base64 encode password");
            return;
        }

        var username = data.Username;
        var password = new string(chars.AsSpan(0, charCount));

        var jsonString = JsonConvert.SerializeObject(new Dictionary<string, string>{{"username", username}, {"password", password}});

        try {
            File.WriteAllText(AccountFilePath, jsonString);
        } catch (Exception e) {
            Logger.Log(LogLevel.Error, $"Failed to write to file {AccountFilePath}: {e.Message}");
        }
    }
    
    #endregion
}

#region SettingTypes

public interface ISettingType {
    string Serialize();
    void Deserialize(string str);
    void ResetToDefault();
}

public class InputSetting(Scancode def = Scancode.Unknown) : ISettingType {
    
    public Scancode Key { get; private set; } = def;

    private readonly Scancode _default = def;

    public void Set(Scancode key) => Key = key;

    public bool Equals(Scancode key) => key == Key;

    public string Serialize() => $"{Key}";

    public void Deserialize(string str) => Key = Enum.Parse<Scancode>(str);

    public void ResetToDefault() {
        Key = _default;
    }
}

public class ValueSetting<T>(T def = default) : ISettingType {

    public T Value = def;

    private readonly T _default = def;

    public string Serialize() {
        if (!IsNumericType<T>() && !typeof(T).IsEnum && typeof(T) != typeof(string) && typeof(T) != typeof(char)) {
            throw new NotSupportedException($"Type '{typeof(T).Name}' not supported");
        }
        return $"{Value}";
    }

    public void Deserialize(string str) {
        if (typeof(T).IsEnum) {
            Value = (T) Enum.Parse(typeof(T), str);
        } else if (typeof(T) == typeof(string)) {
            Value = (T) (object) str;
        } else if (typeof(T) == typeof(char)) {
            Value = (T) (object) str[0];
        } else if (IsNumericType<T>()) {
            Value = (T) Convert.ChangeType(str, typeof(T));
        } else {
            throw new NotSupportedException($"Type '{typeof(T).Name}' not supported");
        }
    }

    public T Get() => Value;

    public void Set(T value) => Value = value;

    public void ResetToDefault() => Value = _default;

    public static implicit operator T(ValueSetting<T> valueSetting) => valueSetting.Value;

    private static bool IsNumericType<TValue>() => Type.GetTypeCode(typeof(TValue)) switch { TypeCode.Boolean or TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal => true, _ => false };
}

#endregion