using System;
using OpenTK.Platform;

namespace MonoClient.State.SettingTypes;

public class InputSetting : ISettingType {
    
    public Scancode Key { get; private set; } = Scancode.Unknown;

    public void Set(Scancode key) {
        Key = key;
    }
    
    public void SetValue(ISettingType newValue) {
        if (newValue is not InputSetting inputValue) {
            return;
        }

        Key = inputValue.Key;
    }

    public bool CheckValue(Scancode key) => key == Key;

    public string Serialize() {
        if (Key != Scancode.Unknown) {
            return $"Scancode.{Key.ToString()}";
        }

        return "None";
    }

    public void Deserialize(string str) {
        if (str == "None") {
            return;
        }

        var split = str.Split('.');

        if (split.Length != 2) {
            throw new Exception($"Invalid input setting: {str}");
        }

        var type = split[0];
        var value = split[1];

        Key = type switch {
            "Scancode" => Enum.Parse<Scancode>(value),
            _ => throw new Exception($"Invalid input setting: {str}")
        };
    }
}