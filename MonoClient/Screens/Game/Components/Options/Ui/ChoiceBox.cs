using System;
using MonoClient.State.SettingTypes;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Options.Ui;

public class ChoiceBox<T> : Sprite {
    private const int BoxWidth = KeyCodeBox.BoxWidth;
    private const int BoxHeight = KeyCodeBox.BoxHeight;

    private readonly ValueSetting<T> _setting;
    private readonly string[] _labels;
    private readonly object[] _values;
    private readonly Action _callback;

    private readonly ColorRect _background;
    private readonly SimpleText _char;
    private int _selected;

    public ChoiceBox(ValueSetting<T> setting, string[] labels, object[] values, Action callback) {
        SetBaseDimensions(BoxWidth, BoxHeight);
        MouseEnabled = true;

        _setting = setting;
        _labels = labels;
        _values = values;
        _callback = callback;

        if (setting != null) {
            for (var i = 0; i < values.Length; i++) {
                if (!setting.Value.Equals((T) values[i])) {
                    continue;
                }

                _selected = i;
                break;
            }
        }

        _background = new ColorRect(new ColorRectConfig { Width = BoxWidth, Height = BoxHeight, Color = 0x444444 });
        AddChild(_background);

        _char = new SimpleText(new TextConfig
            { Text = labels[_selected], FontSize = 25, Bold = true, X = BoxWidth / 2, Y = BoxHeight / 2, OutlineThickness = 2, Anchor = UiAnchor.Middle });
        AddChild(_char);

        AddEventListener(MouseEventId.MouseOver, () => _background.SetColor(11776947));
        AddEventListener(MouseEventId.MouseOut, () => _background.SetColor(4473924));
        AddEventListener(MouseEventId.LeftClick, OnClick);
    }

    private void OnClick() {
        SetSelected(_selected + 1);
        _callback.Invoke();
    }

    private void SetSelected(int selected) {
        _selected = selected >= _values.Length ? 0 : selected;
        _char.SetText(_labels[_selected]);
        _setting?.SetValue((T) _values[_selected]);
    }
}