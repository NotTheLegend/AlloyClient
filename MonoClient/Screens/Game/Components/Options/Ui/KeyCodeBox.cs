using System;
using Common;
using Common.Vector;
using MonoClient.State.SettingTypes;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButton = MonoClient.State.Input.MouseButton;

namespace MonoClient.Screens.Game.Components.Options.Ui;

public class KeyCodeBox : Sprite {
    public const int BoxWidth = 128;
    public const int BoxHeight = 51;

    private static readonly string[] CharCodes = [
        "[Unset]", "", "", "", "", "", "", "", "Backspace", "Tab", "", "", "Clear", "Enter", "", "Cmd", "Shift", "Ctrl", "Alt", "Pause", "CapsLock", "", "", "", "", "", "",
        "Esc", "", "", "", "", "Space", "PgUp", "PgDn", "End", "Home", "Left", "Up", "Right", "Down", "", "", "", "", "Insert", "Delete", "", "0", "1", "2", "3", "4", "5",
        "6", "7", "8", "9", "", "", "", "", "", "", "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X",
        "Y", "Z", "", "Win", "Menu", "", "", "Numpad 0", "Numpad 1", "Numpad 2", "Numpad 3", "Numpad 4", "Numpad 5", "Numpad 6", "Numpad 7", "Numpad 8", "Numpad 9",
        "Numpad *", "Numpad +", "Numpad Enter", "Numpad -", "Numpad .", "Numpad /", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14",
        "F15", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "NumLock", "ScrLock", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ";", "=", ",", "-", ".", "/", "`", "", "", "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "[", "\\", "]", "\'", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "", "", "", "", "", ""
    ];

    private static readonly string[] MouseCodes = ["None", "Left Click", "Right Click", "Middle Click", "Extra 1", "Extra 2"];

    public InputSetting Value;

    private readonly ColorRect _background;
    private readonly SimpleText _char;
    private readonly Action _callback;

    private Keys _keyCode;
    private string _mouseText;
    private bool _inputMode;
    private double _elapsed;

    public KeyCodeBox(InputSetting setting, Action callback) {
        SetBaseDimensions(BoxWidth, BoxHeight);
        SetHitboxType(HitboxType.Custom);
        MouseEnabled = true;

        _callback = callback;

        if (setting.Mouse == MouseButton.None) {
            _keyCode = setting.Key;
        }
        else {
            _mouseText = MouseCodes[(int) setting.Mouse];
        }

        Value = setting;

        _background = new ColorRect(new ColorRectConfig { Width = BoxWidth, Height = BoxHeight, Color = 0x444444 });
        AddChild(_background);

        _char = new SimpleText(new TextConfig {
            Text = _mouseText ?? CharCodes[(int) _keyCode], FontSize = 25, FontType = FontType.Bold, X = BoxWidth / 2, Y = BoxHeight / 2, OutlineThickness = 2, Anchor = UiAnchor.Middle
        });
        AddChild(_char);

        AddEventListener(MouseEvent.MouseOver, () => _background.SetColor(11776947));
        AddEventListener(MouseEvent.MouseOut, () => _background.SetColor(4473924));
        AddEventListener(MouseEvent.LeftClick, OnLeftClick);
        AddEventListener(MouseEvent.RightClick, OnRightClick);
        AddEventListener(MouseEvent.MiddleClick, OnMiddleClick);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (!_inputMode) {
            return;
        }

        _elapsed += gameTime.ElapsedMs;
        if (_elapsed > 500) {
            _char.Visible = !_char.Visible;
            _elapsed = 0;
        }
    }

    protected override bool CustomHitbox(IntVector2 pos) {
        if (_inputMode) {
            return true;
        }

        return pos.X > 0 && pos.X < Width && pos.Y > 0 && pos.Y < Height;
    }

    private void OnLeftClick() {
        if (_inputMode) {
            Main.GameInstance.Window.TextInput -= OnKeyPress;
            Reset();
            return;
        }

        _inputMode = true;
        _char.SetText("[Hit Key]");
        Main.GameInstance.Window.TextInput += OnKeyPress;
    }

    private void OnKeyPress(object _, TextInputEventArgs args) {
        Main.GameInstance.Window.TextInput -= OnKeyPress;

        if (args.Key == Keys.Escape) {
            Reset();
            return;
        }

        _keyCode = args.Key;
        _mouseText = null;
        Value.Key = _keyCode;

        _callback.Invoke();

        Reset();
    }

    private void Reset() {
        _char.Visible = true;
        _char.SetText(_mouseText ?? CharCodes[(int) _keyCode]);
        _elapsed = 0;
        _inputMode = false;
    }

    public void SetValue(InputSetting setting) {
        if (setting.Key != Keys.Unknown) {
            _keyCode = setting.Key;
        }
        else {
            _mouseText = MouseCodes[(int) setting.Mouse];
        }

        Value = setting;
        Reset();
    }

    private void OnRightClick() {
        if (!_inputMode) {
            return;
        }

        Main.GameInstance.Window.TextInput -= OnKeyPress;
        _mouseText = MouseCodes[(int) MouseButton.Right];
        Value.Mouse = MouseButton.Right;

        _callback.Invoke();

        Reset();
    }

    private void OnMiddleClick() {
        if (!_inputMode) {
            return;
        }

        Main.GameInstance.Window.TextInput -= OnKeyPress;
        _mouseText = MouseCodes[(int) MouseButton.Middle];
        Value.Mouse = MouseButton.Middle;

        _callback.Invoke();

        Reset();
    }
}