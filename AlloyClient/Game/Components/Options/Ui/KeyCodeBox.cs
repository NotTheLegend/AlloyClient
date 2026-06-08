using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Enums;
using Alloy.Common;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient.Game.Components.Options.Ui;

public class KeyCodeBox : Sprite {
    public const int BoxWidth = 128;
    public const int BoxHeight = 51;

    private static readonly string[] CharCodes = [ //TODO: sync to Key enum
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

    public InputSetting Value;

    private readonly ColorRect _background;
    private readonly SimpleText _char;
    private readonly Action _callback;

    private Scancode _keyCode;
    private bool _inputMode;
    private double _elapsed;

    public KeyCodeBox(InputSetting setting, Action callback) {
        //todo:SetBaseDimensions(BoxWidth, BoxHeight);
        SetHitboxType(CollisionType.Custom);
        MouseEnabled = true;

        _callback = callback;
        
        _keyCode = setting.Key;

        Value = setting;

        _background = new ColorRect(new ColorRectConfig { Width = BoxWidth, Height = BoxHeight, Color = 0x444444 });
        AddChild(_background);

        _char = new SimpleText(new TextConfig {
            Text = CharCodes[(int)Toolkit.Keyboard.GetKeyFromScancode(_keyCode)], FontSize = 25, FontType = FontType.Bold, X = BoxWidth / 2, Y = BoxHeight / 2, OutlineThickness = 2, Anchor = UiAnchor.Middle
        });
        AddChild(_char);

        AddEventListener(MouseEvent.MouseOver, () => _background.SetColor(11776947));
        AddEventListener(MouseEvent.MouseOut, () => _background.SetColor(4473924));
        AddEventListener(MouseEvent.LeftClick, OnLeftClick);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void OnFrameEnter() {
        if (!_inputMode) {
            return;
        }

        _elapsed += Stage.GameTime.ElapsedMs;
        if (_elapsed > 500) {
            _char.Visible = !_char.Visible;
            _elapsed = 0;
        }
    }

    protected override bool CustomHitbox(Vector2i pos) {
        if (_inputMode) {
            return true;
        }

        return pos.X > 0 && pos.X < Width && pos.Y > 0 && pos.Y < Height;
    }

    private void OnLeftClick() {
        if (_inputMode) {
            RemoveEventListener(KeyboardEvent.KeyUp, OnKeyPress);
            Reset();
            return;
        }

        _inputMode = true;
        _char.SetText("[Hit Key]");
        AddEventListener(KeyboardEvent.KeyUp, OnKeyPress);
    }

    private void OnKeyPress(KeyboardEvent args) {
        RemoveEventListener(KeyboardEvent.KeyUp, OnKeyPress);

        if (args.Code == Scancode.Escape) {
            Reset();
            return;
        }

        _keyCode = args.Code;
        Value.Set(_keyCode);

        _callback.Invoke();

        Reset();
    }

    private void Reset() {
        _char.Visible = true;
        _char.SetText(CharCodes[(int)Toolkit.Keyboard.GetKeyFromScancode(_keyCode)]);
        _elapsed = 0;
        _inputMode = false;
    }

    public void SetValue(InputSetting setting) {
        if (setting.Key != Scancode.Unknown) {
            _keyCode = setting.Key;
        }

        Value = setting;
        Reset();
    }
}