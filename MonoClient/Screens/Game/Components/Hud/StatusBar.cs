using System;
using MonoClient.State;
using MonoClient.Ui;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud;

public class StatusBar : Sprite {
    private readonly int _width;
    private readonly int _height;

    private NineSliceRect _mainBar;
    private NineSliceRect _backgroundBar;
    private NineSliceRect _outlineBar;
    private SimpleText _label;
    private SimpleText _valueText;

    public string labelString;
    private bool _mouseOver;

    public StatusBar(int width, int height, uint color, uint backColor, uint outlineColor, string label) {
        SetBaseDimensions(_width = width, _height = height);

        _backgroundBar = new NineSliceRect(new NineSliceConfig { SliceData = SliceConfig.StatusBar, CutX = 10, CutY = 10, Width = _width, Height = _height });
        _backgroundBar.SetColor(backColor);
        AddChild(_backgroundBar);

        _mainBar = new NineSliceRect(new NineSliceConfig { SliceData = SliceConfig.StatusBar, CutX = 10, CutY = 10, Width = _width, Height = _height });
        _mainBar.SetColor(color);
        AddChild(_mainBar);

        labelString = label;
        _label = new SimpleText(new TextConfig { Text = label, FontSize = 16, FontType = FontType.Bolder, X = 4, Y = _height / 2, OutlineThickness = 1, Color = 0xFFFFFF, OutlineColor = 0xFFFFFF, Anchor = UiAnchor.MiddleLeft });
        AddChild(_label);
        
        _valueText = new SimpleText(new TextConfig { Text = "", FontSize = 16, FontType = FontType.Bold, Y = _height / 2, OutlineThickness = 1, Color = 0xFFFFFF, OutlineColor = 0xFFFFFF, Anchor = UiAnchor.MiddleLeft });
        AddChild(_valueText);

        AddEventListener(MouseEventId.MouseOver, OnMouseOver);
        AddEventListener(MouseEventId.MouseOut, OnMouseOut);
    }

    public void Update(int val, int max, int boost = 0, int baseMax = -1, int level = -1) {
        _mainBar.Resize((int)(_width * (val / (float)max)), _height);

        _valueText.Visible = false;

        if (_mouseOver || Settings.ToggleBarText)
            UpdateText(val, max, boost, baseMax, level);
    }

    public void UpdateLabel(string label)
    {
        if(_label != null)
        {
            RemoveChild(_label);
        }

        labelString = label;
        _label = new SimpleText(new TextConfig { Text = label, FontSize = 16, FontType = FontType.Bolder, X = 4, Y = _height / 2, OutlineThickness = 1, Color = 0xFFFFFF, OutlineColor = 0xFFFFFF, Anchor = UiAnchor.MiddleLeft });
        AddChild(_label);
    }

    private void UpdateText(int val, int max, int boost, int baseMax, int level) {
        _valueText.Visible = true;

        var ltmt = "";

        if (Settings.ToggleLeftToMax) {
            var ltm = baseMax - max - boost;
            if (level >= 20 && ltm > 0)
                ltmt = $"|{Math.Ceiling(ltm / 5f)}";
        }

        if (max > 0)
            _valueText.SetText($"{val}/{max}" + ltmt);
        else
            _valueText.SetText($"{val}");
        _valueText.X = _width / 2 - _valueText.Width / 2;
    }

    private void OnMouseOver() => _mouseOver = true;
    private void OnMouseOut() => _mouseOver = false;
}