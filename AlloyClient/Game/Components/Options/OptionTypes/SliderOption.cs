using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;

namespace AlloyClient.Game.Components.Options.OptionTypes;

public class SliderOption : Option {
    private const int BarWidth = 128;
    private const int BarHeight = 51;
    private const int TrackX = 10;
    private const int TrackWidth = BarWidth - TrackX * 2;

    private readonly ValueSetting<float> _setting;
    private readonly Action<float> _sliderCallback;
    private readonly CutEdgeRect _background;
    private readonly ColorRect _fill;
    private readonly ColorRect _handle;
    private bool _dragging;

    public SliderOption(ValueSetting<float> setting, string text, Action<float> sliderCallback) : base(setting, text, null) {
        _setting = setting;
        _sliderCallback = sliderCallback;

        var outline = new CutEdgeRect(new CutEdgeConfig {
            Width = BarWidth,
            Height = BarHeight,
            CutX = 5,
            CutY = 5,
            Color = 0x5A5A5A
        });
        AddChild(outline);

        _background = new CutEdgeRect(new CutEdgeConfig {
            X = 2,
            Y = 2,
            Width = BarWidth - 4,
            Height = BarHeight - 4,
            CutX = 4,
            CutY = 4,
            Color = 0x3A3A3A
        });
        AddChild(_background);

        var track = new ColorRect(new ColorRectConfig {
            X = TrackX,
            Y = BarHeight / 2 - 2,
            Width = TrackWidth,
            Height = 4,
            Color = 0x1F1F1F
        });
        AddChild(track);

        _fill = new ColorRect(new ColorRectConfig {
            X = TrackX,
            Y = BarHeight / 2 - 2,
            Width = 1,
            Height = 4,
            Color = 0xB3B3B3
        });
        AddChild(_fill);

        _handle = new ColorRect(new ColorRectConfig {
            Y = BarHeight / 2 - 9,
            Width = 6,
            Height = 18,
            Color = 0xFFFFFF
        });
        AddChild(_handle);

        MouseEnabled = true;
        AddEventListener(MouseEvent.LeftDown, OnLeftDown);
        AddEventListener(MouseEvent.MouseMove, OnMouseMove);
        AddEventListener(MouseEvent.LeftUp, OnLeftUp);
        AddEventListener(MouseEvent.MouseOver, OnMouseOver);
        AddEventListener(MouseEvent.MouseOut, OnMouseOut);

        Refresh();
    }

    public override void Refresh() {
        SetVisualValue(_setting.Value);
    }

    public override void SetDisabled(bool val) {
        _disabled = val;
        MouseEnabled = !val;
        Alpha = val ? 0.45f : 1f;
        DescText?.SetColor(val ? 0x666666u : 0xB3B3B3u);
    }

    private void OnLeftDown(MouseEvent args) {
        if (_disabled) {
            return;
        }

        _dragging = true;
        CapturePointer();
        SetFromMouse(args.Coords);
    }

    private void OnMouseMove(MouseEvent args) {
        if (!_dragging) {
            return;
        }

        SetFromMouse(args.Coords);
    }

    private void OnLeftUp(MouseEvent args) {
        if (!_dragging) {
            return;
        }

        SetFromMouse(args.Coords);
        _dragging = false;
        ReleasePointer();
        Settings.SaveSettings();
    }

    private void SetFromMouse(Vector2 coords) {
        var local = GlobalToLocal(coords);
        var value = Math.Clamp((local.X - TrackX) / TrackWidth, 0f, 1f);
        _setting.Set(value);
        SetVisualValue(value);
        _sliderCallback?.Invoke(value);
    }

    private void SetVisualValue(float value) {
        var clamped = Math.Clamp(value, 0f, 1f);
        var filledWidth = Math.Max(1, (int)MathF.Round(TrackWidth * clamped));
        _fill.Width = filledWidth;
        _handle.X = TrackX + filledWidth - _handle.Width / 2;
    }

    private void OnMouseOver() {
        if (!_disabled) {
            _background.SetColor(0x555555);
        }
    }

    private void OnMouseOut() {
        if (!_disabled) {
            _background.SetColor(0x3A3A3A);
        }
    }
}
