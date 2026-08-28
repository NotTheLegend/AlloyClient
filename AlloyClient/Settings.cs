using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AlloyClient.Data;
using Alloy.Common;
using AlloyClient.Logging;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient;

public static class Settings {
    private readonly static ILogger Logger = ILogger.CreateLogger(nameof(Settings));

    private const string LocalFolderName = "AlloyClient";
    private const string AccountFileName = "account.xml";
    private const string SettingsFileName = "settings.xml";

    private readonly static string AccountFilePath;
    private readonly static string SettingsFilePath;

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

    public static Vector2i ScreenSize;
    
    #region HOTKEYS
    
    // Movement
    public readonly static InputSetting MoveUp = new(Scancode.W);
    public readonly static InputSetting MoveDown = new(Scancode.S);
    public readonly static InputSetting MoveLeft = new(Scancode.A);
    public readonly static InputSetting MoveRight = new(Scancode.D);

    // Camera
    public readonly static InputSetting RotateLeft = new(Scancode.Q);
    public readonly static InputSetting RotateRight = new(Scancode.E);
    public readonly static InputSetting ResetCameraAngle = new(Scancode.Z);
    public readonly static InputSetting CenterPlayerKey = new(Scancode.X);

    // Key
    public readonly static InputSetting Options = new(Scancode.Escape);
    public readonly static InputSetting AutoFire = new(Scancode.I);
    public readonly static InputSetting Special = new(Scancode.Spacebar);
    public readonly static InputSetting Interact = new(Scancode.D0);
    public readonly static InputSetting Escape = new(Scancode.R);

    // Chat
    public readonly static InputSetting Chat = new(Scancode.Return);
    public readonly static InputSetting ChatCommand = new(Scancode.QuestionMark);
    public readonly static InputSetting TellKey = new(Scancode.Tab);
    public readonly static InputSetting GuildChat = new(Scancode.G);
    public readonly static InputSetting PartyChat = new(Scancode.P);
    public readonly static InputSetting ChatHistoryUp = new(Scancode.PageUp);
    public readonly static InputSetting ChatHistoryDown = new(Scancode.PageDown);

    // Inventory
    public readonly static InputSetting HealthPotion = new(Scancode.F);
    public readonly static InputSetting MagicPotion = new(Scancode.V);
    public readonly static InputSetting InvOne = new(Scancode.D1);
    public readonly static InputSetting InvTwo = new(Scancode.D2);
    public readonly static InputSetting InvThree = new(Scancode.D3);
    public readonly static InputSetting InvFour = new(Scancode.D4);
    public readonly static InputSetting InvFive = new(Scancode.D5);
    public readonly static InputSetting InvSix = new(Scancode.D6);
    public readonly static InputSetting InvSeven = new(Scancode.D7);
    public readonly static InputSetting InvEight = new(Scancode.D8);
    
    // Misc
    public readonly static InputSetting PerformanceStats = new(Scancode.F5);
    public readonly static InputSetting SwitchTabs = new(Scancode.B);
    public readonly static InputSetting ResetMScale = new(Scancode.Unknown);
    public readonly static InputSetting SetBagPriority = new(Scancode.Unknown);
    public readonly static InputSetting FullscreenKey = new(Scancode.F11);
    
    #endregion
    
    #region VALUES

    // Random
    public readonly static ValueSetting<PacketLogLevel> PacketLogging = new(PacketLogLevel.Off);
    public readonly static ValueSetting<ushort> SelectedGameServerPort = new(GameServerPort);
    
    // Camera
    public readonly static ValueSetting<int> MaxRenderDistance = new(20);
    public readonly static ValueSetting<bool> CenterPlayer = new(true);
    public readonly static ValueSetting<float> CameraAngle = new(0f);
    public readonly static ValueSetting<float> CameraZoom = new(1f);
    public readonly static ValueSetting<bool> AllowRotation = new(true);
    public readonly static ValueSetting<float> RotateSpeed = new(0.003f);

    // Screen
    public readonly static ValueSetting<int> FpsCap = new(-1);
    public readonly static ValueSetting<bool> VSync = new(false);
    public readonly static ValueSetting<WindowMode> LastWindowMode = new(WindowMode.Normal);
    public readonly static ValueSetting<int> LastWindowPositionX = new(0);
    public readonly static ValueSetting<int> LastWindowPositionY = new(0);
    public readonly static ValueSetting<int> LastWindowWidth = new(DefaultScreenWidth);
    public readonly static ValueSetting<int> LastWindowHeight = new(DefaultScreenHeight);
    public readonly static ValueSetting<FullscreenType> FullscreenMode = new(FullscreenType.Borderless);
    public readonly static ValueSetting<bool> FullscreenState = new(false);

    // Audio
    public readonly static ValueSetting<float> MasterVolume = new(0.5f);
    public readonly static ValueSetting<float> MusicVolume = new(1f);
    public readonly static ValueSetting<float> SfxVolume = new(1f);
    public readonly static ValueSetting<bool> PlayMaster = new(true);
    public readonly static ValueSetting<bool> PlayMusic = new(true);
    public readonly static ValueSetting<bool> PlaySfx = new(true);
    
    // Chat
    public readonly static ValueSetting<int> ChatInclude = new(0);
    public readonly static ValueSetting<bool> ChatVisible = new(true);
    public readonly static ValueSetting<float> ChatScaling = new(1f);
    public readonly static ValueSetting<int> ChatHideList = new(0);

    // Particles
    public readonly static ValueSetting<bool> EyeCandyParticles = new(true);
    public readonly static ValueSetting<bool> ReducedParticles = new(false);

    // Other
    public readonly static ValueSetting<bool> ToggleLeftToMax = new(true);
    public readonly static ValueSetting<bool> ToggleBarText = new(true);
    public readonly static ValueSetting<bool> InventorySwap = new(true);
    public readonly static ValueSetting<bool> MovementInterpolation = new(true);
    public readonly static ValueSetting<int> HealthBars = new(1);
    public readonly static ValueSetting<bool> DrawMpBar = new(false);
    
    #endregion
    
    public readonly static InputSetting[] Inputs;

    private readonly static ReadOnlyDictionary<string, ISettingType> SettingsLookup;

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
            Logger.Log(LogLevel.Trace, "Settings file not found.");
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
        
        Logger.Log(LogLevel.Trace, $"Loaded {count} of {SettingsLookup.Count} settings, {SettingsLookup.Count - count} reset to default");
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
            Logger.Log(LogLevel.Trace, $"Saved {count} of {SettingsLookup.Count} settings");
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
        
        var xml = XDocument.Parse(text);
        var info = xml.Root?.Elements().ToDictionary(e => e.Name.LocalName, e => e.Value);
        
        if (info == null) {
            Logger.Log(LogLevel.Debug, "Failed to parse local account data");
            return;
        }

        var loadedUser = info.TryGetValue("Username", out var username) && !string.IsNullOrWhiteSpace(username);
        var loadedPass = info.TryGetValue("Password", out var password) && !string.IsNullOrWhiteSpace(password);

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

        var tags = new Dictionary<string, string>{{"Username", username}, {"Password", password}};
        var xml = new XDocument(new XElement("Account", tags.Select(kvp => new XElement(kvp.Key, kvp.Value))));

        try {
            xml.Save(AccountFilePath);
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
    
    public override string ToString() => $"{Key}";
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

    public override string ToString() => $"{Value}";
}

#endregion
