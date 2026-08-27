using Alloy.Engine;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Input;
using Alloy.UiLib.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform;
using MouseState = Alloy.UiLib.Input.MouseState;

namespace Alloy.UiLib.Core;

/// <summary>
/// This is the layer zero sprite that provides access to sprites internal Update/Draw functions, there can only be one
/// </summary>
public sealed class Stage : Sprite {

    public Vector2 ScreenScale { get; private set; }

    public Vector2i Dimensions => new (StageWidth, StageHeight);
    
    public int StageWidth { get; private set; }
    
    public int StageHeight { get; private set; }
    
    public static GameTime GameTime { get; private set; } // todo: make not static
    
    public KeyboardState Keyboard => _keyboard;
    private KeyboardState _keyboard;
    private ManualTextInput _manualTextInput;
    
    public MouseState Mouse => _mouse;
    private MouseState _mouse;

    internal Sprite CurrentHighestSprite;
    private Sprite _lastHighestSprite;

    private Sprite _leftClickTarget;
    private Sprite _middleClickTarget;
    private Sprite _rightClickTarget;

    internal Stage() {
        MouseEnabled = true;
        Stage = this;
    }

    internal void SetSize(Vector2i dim, Vector2 scale) {
        StageWidth = dim.X;
        StageHeight = dim.Y;
        ScreenScale = scale;
    }
    
    public void Update(GameTime gameTime) {
        GameTime = gameTime;
        GTween.Update(gameTime);
        Timer.Update(gameTime);

        if (CurrentHighestSprite != _lastHighestSprite) {
            CurrentHighestSprite?.DispatchEvent(new MouseEvent(MouseEvent.MouseOver, _mouse.GetMousePosition(), _mouse.GetScrollDelta(), _keyboard.IsShiftDown(), _keyboard.IsCtrlDown(), _keyboard.IsAltDown()));
            _lastHighestSprite?.DispatchEvent(new MouseEvent(MouseEvent.MouseOut, _mouse.GetMousePosition(), _mouse.GetScrollDelta(), _keyboard.IsShiftDown(), _keyboard.IsCtrlDown(), _keyboard.IsAltDown()));
            _lastHighestSprite = CurrentHighestSprite;
        }

        CurrentHighestSprite = null;
        
        InternalUpdateLoop();
    }

    public void Draw(GameTime gameTime) {
        InternalDrawLoop();
    }
    
    internal void SetKeyDown(Key key, Scancode scancode) {
        if (_keyboard.SetKeyDown(key)) {
            DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyDown, key, scancode, _keyboard.IsCtrlDown(), _keyboard.IsShiftDown(), _keyboard.IsAltDown()));
        }

        if (_manualTextInput.OnManualTextInputDown(key, GameTime.TotalMs)) {
            TextInput.ActiveInput?.OnManualTextInput(key);
        }
    }

    internal void SetKeyUp(Key key, Scancode scancode) {
        if (_keyboard.SetKeyUp(key)) {
            DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyUp, key, scancode, _keyboard.IsCtrlDown(), _keyboard.IsShiftDown(), _keyboard.IsAltDown()));
        }
        
        _manualTextInput.OnManualTextInputUp(key);
    }

    internal void SetMouseButtonDown(MouseButton button) {
        if (!_mouse.SetButtonDown(button)) {
            return;
        }

        switch (button) {
            case MouseButton.Button1: _leftClickTarget = _lastHighestSprite; break;
            case MouseButton.Button2: _middleClickTarget = _lastHighestSprite; break;
            case MouseButton.Button3: _rightClickTarget = _lastHighestSprite; break;
        }
            
        DispatchMouseEvent(button.AsEventType(true));
    }
    
    internal void SetMouseButtonUp(MouseButton button) {
        if (!_mouse.SetButtonUp(button)) {
            return;
        }
        
        DispatchMouseEvent(button.AsEventType(false));

        switch (button) {
            case MouseButton.Button1 when _leftClickTarget == _lastHighestSprite:
                if (TextInput.ActiveInput != null && _lastHighestSprite != TextInput.ActiveInput) {
                    TextInput.ActiveInput.UnFocus();
                }
                DispatchMouseEvent(MouseEvent.LeftClick);
                break;
            case MouseButton.Button2 when _middleClickTarget == _lastHighestSprite: DispatchMouseEvent(MouseEvent.MiddleClick); break;
            case MouseButton.Button3 when _rightClickTarget == _lastHighestSprite: DispatchMouseEvent(MouseEvent.RightClick); break;
        }
    }

    internal void SetMouseScroll(Vector2 delta) {
        _mouse.SetScrollDelta(delta);

        if (delta.X != 0) {
            DispatchMouseEvent(MouseEvent.ScrollHorizontal);
        }

        if (delta.Y != 0) {
            DispatchMouseEvent(MouseEvent.ScrollVertical);
        }
    }

    internal void SetMousePosition(Vector2 position) {
        _mouse.SetPosition(position);
        DispatchMouseEvent(MouseEvent.MouseMove);
    }

    internal void ResetMouseState() {
        _mouse = default;
        _leftClickTarget = null;
        _middleClickTarget = null;
        _rightClickTarget = null;
    }

    private void DispatchMouseEvent(EventType<MouseEvent> type) {
        if (_lastHighestSprite is null) {
            return;
        }

        var args = new MouseEvent(type, _mouse.GetMousePosition(), _mouse.GetScrollDelta(), _keyboard.IsShiftDown(), _keyboard.IsCtrlDown(), _keyboard.IsAltDown());
        _lastHighestSprite.DispatchEvent(args);
    }
}