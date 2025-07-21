using System;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButton = MonoClient.State.Input.MouseButton;

namespace MonoClient.State.SettingTypes;

public class InputSetting : ISettingType {
    private Keys _key = Keys.Unknown;

    public Keys Key {
        get => _key;
        set {
            _key = value;
            _mouse = MouseButton.None;
        }
    }

    private MouseButton _mouse = MouseButton.None;

    public MouseButton Mouse {
        get => _mouse;
        set {
            _mouse = value;
            _key = Keys.Unknown;
        }
    }

    public string Serialize() {
        if (_key != Keys.Unknown) {
            return $"Keys.{_key.ToString()}";
        }

        if (_mouse != MouseButton.None) {
            return $"MouseButton.{_mouse.ToString()}";
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

        switch (type) {
            case "Keys":
                Key = Enum.Parse<Keys>(value);
                break;
            case "MouseButton":
                Mouse = Enum.Parse<MouseButton>(value);
                break;
            default:
                throw new Exception($"Invalid input setting: {str}");
        }
    }

    public void SetValue(ISettingType newValue) {
        if (newValue is not InputSetting inputValue) {
            return;
        }

        _key = inputValue.Key;
        _mouse = inputValue.Mouse;
    }
}