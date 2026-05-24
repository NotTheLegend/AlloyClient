using System;
using System.Diagnostics;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using OpenTK.Platform;

namespace Alloy.UiLib.Input;

public static class KeyboardInput {
    
    private static InternalKeyboardState _internalState;

    private static Stage _stage;
    
    public static bool IsKeyDown(Key key) {
        return _internalState.IsKeyDown(key);
    }
    
    public static bool IsKeyUp(Key key) {
        return _internalState.IsKeyUp(key);
    }

    public static bool IsShiftDown() => IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift);
    
    public static bool IsAltDown() => IsKeyDown(Key.LeftAlt) || IsKeyDown(Key.RightAlt);
    
    public static bool IsCtrlDown() => IsKeyDown(Key.LeftControl) || IsKeyDown(Key.RightControl);

    public static bool IsOnlyCtrlDown() => IsCtrlDown() && !IsShiftDown() && !IsAltDown();

    internal static void Register(Stage stage) {
        _stage = stage;
        _internalState = new InternalKeyboardState();
    }

    internal static void SetKeyDown(Key key, Scancode scancode) {
        if (_internalState.IsKeyUp(key)) {
            _internalState.SetKey(key);
            _stage.DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyDown, key, scancode, IsCtrlDown(), IsShiftDown(), IsAltDown()));
        }
        
        OnManualTextInputDown(key);
    }

    internal static void SetKeyUp(Key key, Scancode scancode) {
        if (_internalState.IsKeyDown(key)) {
            _internalState.ClearKey(key);
            _stage.DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyUp, key, scancode, IsCtrlDown(), IsShiftDown(), IsAltDown()));
        }
        
        OnManualTextInputUp(key);
    }

    internal static void OnTextInput(ReadOnlySpan<char> text) {
        TextInput.ActiveInput?.OnTextInput(text);
    }

    private static void OnManualTextInputDown(Key key) {
        var time = Stopwatch.Elapsed.TotalMilliseconds;
        if (key != _lastKeyDown) {
            _lastKeyDown = key;
            _nextTickTime = time + InitDelay;
            TextInput.ActiveInput?.OnManualTextInput(key);
            return;
        }

        if (time < _nextTickTime) {
            return;
        }

        _nextTickTime = time + RepeatDelay;
        TextInput.ActiveInput?.OnManualTextInput(key);
    }
    
    private static void OnManualTextInputUp(Key key) {
        if (key != _lastKeyDown) {
            return;
        }

        _lastKeyDown = Key.Unknown;
    }

    private const double InitDelay = 500; // ms
    private const double RepeatDelay = 33; //ms

    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
    private static Key _lastKeyDown;

    private static double _nextTickTime;

}

internal struct InternalKeyboardState {
    private uint _keys0;
    private uint _keys1;
    private uint _keys2;
    private uint _keys3;
    private uint _keys4;
    private uint _keys5;
    private uint _keys6;
    private uint _keys7;

    private bool GetKey(Key key) {
        var num1 = 1u << ((int) key & 31 & 31);
        var num2 = ((int) key >> 5) switch {
            0 => _keys0,
            1 => _keys1,
            2 => _keys2,
            3 => _keys3,
            4 => _keys4,
            5 => _keys5,
            6 => _keys6,
            7 => _keys7,
            _ => 0U
        };

        return (num2 & num1) > 0U;
    }

    internal void SetKey(Key key) {
        var num = 1u << ((int) key & 31 & 31);
        switch ((int) key >> 5) {
            case 0: _keys0 |= num;
                break;
            case 1: _keys1 |= num;
                break;
            case 2: _keys2 |= num;
                break;
            case 3: _keys3 |= num;
                break;
            case 4: _keys4 |= num;
                break;
            case 5: _keys5 |= num;
                break;
            case 6: _keys6 |= num;
                break;
            case 7: _keys7 |= num;
                break;
        }
    }
    
    internal void ClearKey(Key key)
    {
        var num = 1U << ((int)key & 31 & 31);
        switch ((int) key >> 5)
        {
            case 0: _keys0 &= ~num;
                break;
            case 1: _keys1 &= ~num;
                break;
            case 2: _keys2 &= ~num;
                break;
            case 3: _keys3 &= ~num;
                break;
            case 4: _keys4 &= ~num;
                break;
            case 5: _keys5 &= ~num;
                break;
            case 6: _keys6 &= ~num;
                break;
            case 7: _keys7 &= ~num;
                break;
        }
    }

    public bool IsKeyDown(Key key) => GetKey(key);

    public bool IsKeyUp(Key key) => !GetKey(key);

    public int GetPressedKeyCount() {
        return CountBits(_keys0) + CountBits(_keys1) + CountBits(_keys2) + CountBits(_keys3) + CountBits(_keys4) + CountBits(_keys5) + CountBits(_keys6) + CountBits(_keys7);
    }

    private static int CountBits(uint v) {
        v -= v >> 1 & 1431655765U;
        v = (uint) (((int) v & 858993459) + ((int) (v >> 2) & 858993459));
        return ((int) v + (int) (v >> 4) & 252645135) * 16843009 >>> 24;
    }
}