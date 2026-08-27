using System;
using System.Collections.Generic;
using Alloy.Engine;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Input;
using Alloy.UiLib.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform;
using MouseState = Alloy.UiLib.Input.MouseState;

namespace Alloy.UiLib.Core;

public sealed class Stage : Sprite {
    public Vector2 ScreenScale { get; private set; }
    public Vector2i Dimensions => new(StageWidth, StageHeight);
    public int StageWidth { get; private set; }
    public int StageHeight { get; private set; }
    public static GameTime GameTime { get; private set; }
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
    private Sprite _leftPointerCapture;
    private Sprite _middlePointerCapture;
    private Sprite _rightPointerCapture;
    private Sprite _focus;
    internal bool PointerPositionValid = true;

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
        CurrentHighestSprite = null;
        InternalUpdateLoop();
        UpdateHoverTarget();
    }

    public void Draw(GameTime gameTime) {
        InternalDrawLoop();
    }

    internal void SetKeyDown(Key key, Scancode scancode) {
        if (_keyboard.SetKeyDown(key)) {
            var keyEvent = new KeyboardEvent(KeyboardEvent.KeyDown, key, scancode, _keyboard.IsCtrlDown(), _keyboard.IsShiftDown(),
                _keyboard.IsAltDown());

            DispatchKeyboardEvent(keyEvent);

            if (key == Key.Tab && !keyEvent.IsDefaultPrevented()) {
                MoveFocus(keyEvent.Shift);
            }
        }

        if (_manualTextInput.OnManualTextInputDown(key, GameTime.TotalMs) && _focus is IManualTextInputTarget target) {
            target.OnManualTextInput(key);
        }
    }

    internal void SetKeyUp(Key key, Scancode scancode) {
        if (_keyboard.SetKeyUp(key)) {
            DispatchKeyboardEvent(new KeyboardEvent(KeyboardEvent.KeyUp, key, scancode, _keyboard.IsCtrlDown(), _keyboard.IsShiftDown(),
                _keyboard.IsAltDown()));
        }

        _manualTextInput.OnManualTextInputUp(key);
    }

    internal void SetTextInput(ReadOnlySpan<char> text) {
        if (_focus is ITextInputTarget target) {
            target.OnTextInput(text);
        }
    }

    internal void SetMouseButtonDown(MouseButton button) {
        if (!_mouse.SetButtonDown(button)) {
            return;
        }

        switch (button) {
            case MouseButton.Button1:
                _leftClickTarget = _lastHighestSprite;
                SetFocus(FindPointerFocusable(_lastHighestSprite));
                break;
            case MouseButton.Button2:
                _rightClickTarget = _lastHighestSprite;
                break;
            case MouseButton.Button3:
                _middleClickTarget = _lastHighestSprite;
                break;
        }

        DispatchMouseEvent(button.AsEventType(true), GetPointerCapture(button));
    }

    internal void SetMouseButtonUp(MouseButton button) {
        if (!_mouse.SetButtonUp(button)) {
            return;
        }

        DispatchMouseEvent(button.AsEventType(false), GetPointerCapture(button));

        switch (button) {
            case MouseButton.Button1 when _leftClickTarget == _lastHighestSprite:
                DispatchMouseEvent(MouseEvent.LeftClick);
                break;
            case MouseButton.Button2 when _rightClickTarget == _lastHighestSprite:
                DispatchMouseEvent(MouseEvent.RightClick);
                break;
            case MouseButton.Button3 when _middleClickTarget == _lastHighestSprite:
                DispatchMouseEvent(MouseEvent.MiddleClick);
                break;
        }

        ReleasePointer(null, button);
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
        PointerPositionValid = true;
        _mouse.SetPosition(position);
        CurrentHighestSprite = null;
        ResolvePointerTarget();
        UpdateHoverTarget();
        DispatchMouseEvent(MouseEvent.MouseMove, GetActivePointerCapture());
    }

    internal void SetWindowFocus(bool focused) {
        if (!focused && _lastHighestSprite is not null) {
            _lastHighestSprite.DispatchEvent(new MouseEvent(MouseEvent.MouseOut, _mouse.GetMousePosition(), _mouse.GetScrollDelta(),
                _keyboard.IsShiftDown(), _keyboard.IsCtrlDown(), _keyboard.IsAltDown()));
        }

        PointerPositionValid = focused;
        CurrentHighestSprite = null;
        _lastHighestSprite = null;
        _leftClickTarget = null;
        _middleClickTarget = null;
        _rightClickTarget = null;
        _leftPointerCapture = null;
        _middlePointerCapture = null;
        _rightPointerCapture = null;
    }

    public Sprite GetFocus() {
        return _focus;
    }

    public void SetFocus(Sprite focus) {
        if (focus is not null && !focus.CanReceiveFocus()) {
            return;
        }

        if (_focus == focus) {
            return;
        }

        var previous = _focus;
        _focus = focus;
        previous?.DispatchEvent(new FocusEvent(FocusEvent.FocusOut, focus));
        focus?.DispatchEvent(new FocusEvent(FocusEvent.FocusIn, previous));
    }

    public void ClearFocus() {
        SetFocus(null);
    }

    internal void ClearFocusWithin(Sprite root) {
        if (_focus is not null && root.Contains(_focus)) {
            ClearFocus();
        }
    }

    internal void CapturePointer(Sprite sprite, MouseButton button) {
        if (sprite?.Stage == this) {
            SetPointerCapture(button, sprite);
        }
    }

    internal void ReleasePointer(Sprite sprite, MouseButton button) {
        var capture = GetPointerCapture(button);
        if (sprite is null || capture == sprite) {
            SetPointerCapture(button, null);
        }
    }

    internal void ReleasePointersWithin(Sprite root) {
        if (_leftPointerCapture is not null && root.Contains(_leftPointerCapture)) {
            _leftPointerCapture = null;
        }

        if (_middlePointerCapture is not null && root.Contains(_middlePointerCapture)) {
            _middlePointerCapture = null;
        }

        if (_rightPointerCapture is not null && root.Contains(_rightPointerCapture)) {
            _rightPointerCapture = null;
        }
    }

    private void DispatchKeyboardEvent(KeyboardEvent keyEvent) {
        if (_focus is not null && _focus.Stage == this) {
            _focus.DispatchEvent(keyEvent);
            return;
        }

        DispatchEvent(keyEvent);
    }

    private void DispatchMouseEvent(EventType<MouseEvent> type, Sprite target = null) {
        target ??= _lastHighestSprite;
        if (target is null) {
            return;
        }

        var captured = target != _lastHighestSprite;
        var args = new MouseEvent(type, _mouse.GetMousePosition(), _mouse.GetScrollDelta(), _keyboard.IsShiftDown(),
            _keyboard.IsCtrlDown(), _keyboard.IsAltDown(), captured);

        target.DispatchEvent(args);
    }

    private void UpdateHoverTarget() {
        if (CurrentHighestSprite == _lastHighestSprite) {
            return;
        }

        var mousePosition = _mouse.GetMousePosition();
        var scrollDelta = _mouse.GetScrollDelta();
        var shift = _keyboard.IsShiftDown();
        var ctrl = _keyboard.IsCtrlDown();
        var alt = _keyboard.IsAltDown();
        _lastHighestSprite?.DispatchEvent(new MouseEvent(MouseEvent.MouseOut, mousePosition, scrollDelta, shift, ctrl, alt));
        CurrentHighestSprite?.DispatchEvent(new MouseEvent(MouseEvent.MouseOver, mousePosition, scrollDelta, shift, ctrl, alt));
        _lastHighestSprite = CurrentHighestSprite;
    }

    private Sprite FindPointerFocusable(Sprite sprite) {
        while (sprite is not null) {
            if (sprite.PointerFocusEnabled && sprite.CanReceiveFocus()) {
                return sprite;
            }

            sprite = sprite.Parent;
        }

        return null;
    }

    private void MoveFocus(bool backwards) {
        var focusable = new List<(Sprite Sprite, int Order)>();
        var order = 0;
        CollectFocusable(this, focusable, ref order);
        focusable.Sort((left, right) => {
            var leftIndex = left.Sprite.TabIndex < 0 ? int.MaxValue : left.Sprite.TabIndex;
            var rightIndex = right.Sprite.TabIndex < 0 ? int.MaxValue : right.Sprite.TabIndex;
            var comparison = leftIndex.CompareTo(rightIndex);
            return comparison != 0 ? comparison : left.Order.CompareTo(right.Order);
        });

        if (focusable.Count == 0) {
            return;
        }

        var current = focusable.FindIndex(item => item.Sprite == _focus);
        var next = current < 0
            ? backwards ? focusable.Count - 1 : 0
            : (current + (backwards ? -1 : 1) + focusable.Count) % focusable.Count;

        SetFocus(focusable[next].Sprite);
    }

    private static void CollectFocusable(Sprite root, List<(Sprite Sprite, int Order)> focusable, ref int order) {
        if (root.TabEnabled && root.CanReceiveFocus()) {
            focusable.Add((root, order));
        }

        order++;
        foreach (var child in root.GetChildrenSpan()) {
            CollectFocusable(child, focusable, ref order);
        }
    }

    private Sprite GetActivePointerCapture() {
        if (_mouse.IsButtonDown(MouseButton.Button1)) {
            return _leftPointerCapture;
        }

        if (_mouse.IsButtonDown(MouseButton.Button3)) {
            return _middlePointerCapture;
        }

        return _mouse.IsButtonDown(MouseButton.Button2) ? _rightPointerCapture : null;
    }

    private Sprite GetPointerCapture(MouseButton button) {
        return button switch {
            MouseButton.Button1 => _leftPointerCapture,
            MouseButton.Button2 => _rightPointerCapture,
            MouseButton.Button3 => _middlePointerCapture,
            _ => null
        };
    }

    private void SetPointerCapture(MouseButton button, Sprite sprite) {
        switch (button) {
            case MouseButton.Button1:
                _leftPointerCapture = sprite;
                break;
            case MouseButton.Button2:
                _rightPointerCapture = sprite;
                break;
            case MouseButton.Button3:
                _middlePointerCapture = sprite;
                break;
        }
    }
}