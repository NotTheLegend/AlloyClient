using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;

namespace AlloyClient.Game.Components.Options.Ui;

public class ChoiceBox<T> : Sprite {
    private const int BoxWidth = KeyCodeBox.BoxWidth;
    private const int BoxHeight = KeyCodeBox.BoxHeight;

    private readonly ValueSetting<T> _setting;
    private readonly string[] _labels;
    private readonly object[] _values;
    private readonly Action _callback;

    private readonly CutEdgeRect _background;
    private readonly SimpleText _char;
    private int _selected;
    private bool _disabled;

    public ChoiceBox(ValueSetting<T> setting, string[] labels, object[] values, Action callback) {
        //todo:SetBaseDimensions(BoxWidth, BoxHeight);
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

        var outline = new CutEdgeRect(new CutEdgeConfig {
            Width = BoxWidth,
            Height = BoxHeight,
            CutX = 5,
            CutY = 5,
            Color = 0x5A5A5A
        });
        AddChild(outline);

        _background = new CutEdgeRect(new CutEdgeConfig {
            X = 2,
            Y = 2,
            Width = BoxWidth - 4,
            Height = BoxHeight - 4,
            CutX = 4,
            CutY = 4,
            Color = 0x3A3A3A
        });
        AddChild(_background);

        _char = new SimpleText(new TextConfig
            { Text = labels[_selected], FontSize = 25, FontType = FontType.Bold, X = BoxWidth / 2, Y = BoxHeight / 2, OutlineThickness = 2, Anchor = UiAnchor.Middle });
        AddChild(_char);

        AddEventListener(MouseEvent.MouseOver, () => _background.SetColor(0x555555));
        AddEventListener(MouseEvent.MouseOut, () => _background.SetColor(0x3A3A3A));
        AddEventListener(MouseEvent.LeftClick, OnClick);
    }

    private void OnClick() {
        if (_disabled) {
            return;
        }

        SetSelected(_selected + 1);
        _callback.Invoke();
        Settings.SaveSettings();
    }

    private void SetSelected(int selected) {
        _selected = selected >= _values.Length ? 0 : selected;
        _char.SetText(_labels[_selected]);
        _setting?.Set((T) _values[_selected]);
    }

    public void Refresh() {
        if (_setting is null) {
            return;
        }

        for (var i = 0; i < _values.Length; i++) {
            if (!_setting.Value.Equals((T)_values[i])) {
                continue;
            }

            _selected = i;
            _char.SetText(_labels[_selected]);
            return;
        }
    }

    public void SetDisabled(bool disabled) {
        _disabled = disabled;
        MouseEnabled = !disabled;
        Alpha = disabled ? 0.45f : 1f;
        _background.SetColor(disabled ? 0x303030u : 0x3A3A3Au);
    }
}
