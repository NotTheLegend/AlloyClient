using System;
using Common;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;

namespace MonoClient.UiLib.BuiltIn.Buttons;

public struct TextButtonConfig {
    public string Text = "";
    public float FontSize = 1f;
    public Action OnClicked = null;
    public FontType FontType = FontType.Bold;
    public uint ActiveColor = 0xFFFFFF;
    public uint HoverColor = 0xFFDC85;
    public uint InactiveColor = 0x363636;
    public int X = 0;
    public int Y = 0;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public TextButtonConfig() { }
}

public class TextButton : Sprite {
    private readonly uint _activeColor;
    private readonly uint _onHoverColor;
    private readonly uint _inactive;
    
    private readonly SimpleText _text;
    private readonly Action _onClicked;
    
    private bool _leftDown;
    
    public string Name {
        get => _text.Text;
    }
    
    public TextButton(TextButtonConfig config) {
        _activeColor = config.ActiveColor;
        _onHoverColor = config.HoverColor;
        _inactive = config.InactiveColor;
        _text = new SimpleText(new TextConfig {Text = config.Text, FontSize = config.FontSize, FontType = config.FontType, Color = _activeColor});
        _onClicked = config.OnClicked;

        X = config.X;
        Y = config.Y;
        Alpha = config.Alpha;
        SetAnchor(config.Anchor);
        
        MouseEnabled = true;

        AddChild(_text);
        Activate();
    }

    public void SetState(bool state) {
        if (state) Activate();
        else Deactivate();
    }

    public void Activate() {
        _text.SetColor(_activeColor);
        AddEventListener(MouseEventId.MouseOver, OnMouseOver);
        AddEventListener(MouseEventId.MouseOut, OnMouseOut);
        AddEventListener(MouseEventId.LeftDown, OnLeftDown);
        AddEventListener(MouseEventId.LeftUp, OnLeftUp);
    }

    public void Deactivate() {
        _text.SetColor(_inactive);
        RemoveEventListener(MouseEventId.MouseOver, OnMouseOver);
        RemoveEventListener(MouseEventId.MouseOut, OnMouseOut);
        RemoveEventListener(MouseEventId.LeftDown, OnLeftDown);
        RemoveEventListener(MouseEventId.LeftUp, OnLeftUp);
    }

    private void OnMouseOver() {
        _text.SetColor(_onHoverColor);
    }

    private void OnMouseOut() {
        _text.SetColor(_activeColor);
    }
    
    private void OnLeftDown() {
        _leftDown = true;
    }

    private void OnLeftUp() {
        if (_leftDown) {
            _onClicked?.Invoke();
        }
        
        _leftDown = false;
    }
}