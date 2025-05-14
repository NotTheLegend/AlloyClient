using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;

namespace MonoClient.UiLib.Input;

public static class KeyboardInput {
    
    private static InternalKeyboardState _internalState;

    private static Game _game;

    private static Stage _stage;
    
    public static bool IsKeyDown(Keys key) {
        return _internalState.IsKeyDown(key);
    }
    
    public static bool IsKeyUp(Keys key) {
        return _internalState.IsKeyUp(key);
    }

    public static bool IsShiftDown() => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
    
    public static bool IsAltDown() => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);
    
    public static bool IsCtrlDown() => IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);

    internal static void Register(Game game, Stage stage) {
        if (_game != null) return;
        
        game.Window.KeyDown += OnKeyDown;
        game.Window.KeyUp += OnKeyUp;
        game.Window.TextInput += OnTextInput;

        _game = game;
        _stage = stage;
        _internalState = new InternalKeyboardState();
    }

    private static void OnKeyDown(object _, InputKeyEventArgs args) {
        if (!_game.IsActive || _internalState.IsKeyDown(args.Key)) return;
        _internalState.SetKey(args.Key);
        _stage.DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyDown, args.Key, IsCtrlDown(), IsShiftDown(), IsAltDown()));
    }

    private static void OnKeyUp(object _, InputKeyEventArgs args) {
        if (!_game.IsActive || _internalState.IsKeyUp(args.Key)) return;
        _internalState.ClearKey(args.Key);
        _stage.DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyUp, args.Key, IsCtrlDown(), IsShiftDown(), IsAltDown()));
    }

    private static void OnTextInput(object _, TextInputEventArgs args) {
        if (!_game.IsActive) return;
        TextInput.ActiveInput?.OnTextInput(args);
    }
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

    private bool GetKey(Keys key) {
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

    internal void SetKey(Keys key) {
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
    
    internal void ClearKey(Keys key)
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

    public bool IsKeyDown(Keys key) => GetKey(key);

    public bool IsKeyUp(Keys key) => !GetKey(key);

    public int GetPressedKeyCount() {
        return CountBits(_keys0) + CountBits(_keys1) + CountBits(_keys2) + CountBits(_keys3) + CountBits(_keys4) + CountBits(_keys5) + CountBits(_keys6) + CountBits(_keys7);
    }

    private static int CountBits(uint v) {
        v -= v >> 1 & 1431655765U;
        v = (uint) (((int) v & 858993459) + ((int) (v >> 2) & 858993459));
        return ((int) v + (int) (v >> 4) & 252645135) * 16843009 >>> 24;
    }
}