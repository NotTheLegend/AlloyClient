using OpenTK.Platform;

namespace Alloy.UiLib.Input;

public struct KeyboardState {
    
    private InternalState _internalState;
    
    public bool IsKeyDown(Key key) {
        return _internalState.IsDown(key);
    }
    
    public bool IsKeyUp(Key key) {
        return _internalState.IsUp(key);
    }

    public bool IsShiftDown() => IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift);
    
    public bool IsAltDown() => IsKeyDown(Key.LeftAlt) || IsKeyDown(Key.RightAlt);
    
    public bool IsCtrlDown() => IsKeyDown(Key.LeftControl) || IsKeyDown(Key.RightControl);

    public bool IsOnlyCtrlDown() => IsCtrlDown() && !IsShiftDown() && !IsAltDown();
    
    public bool IsOnlyShiftDown() => !IsCtrlDown() && IsShiftDown() && !IsAltDown();
    
    public bool IsOnlyAltDown() => !IsCtrlDown() && !IsShiftDown() && IsAltDown();

    internal bool SetKeyDown(Key key) {
        if (!_internalState.IsUp(key)) {
            return false;
        }
        
        _internalState.Set(key);
        return true;
    }

    internal bool SetKeyUp(Key key) {
        if (!_internalState.IsDown(key)) {
            return false;
        }
        
        _internalState.Clear(key);
        return true;
    }
    
    [System.Runtime.CompilerServices.InlineArray(8)]
    private struct InternalState {
        private uint _;

        private readonly bool Get(Key key) => (this[(int)key >> 5] & (1u << ((int)key & 31))) > 0;

        public void Set(Key key) => this[(int) key >> 5] |= 1u << ((int) key & 31);

        public void Clear(Key key) => this[(int) key >> 5] &= ~(1u << ((int) key & 31));
        
        public readonly bool IsDown(Key key) => Get(key);

        public readonly bool IsUp(Key key) => !Get(key);
    }
}

internal struct ManualTextInput {
    
    private const double InitDelay = 500; // ms
    private const double RepeatDelay = 33; //ms
    
    private Key _lastKeyDown;
    private double _nextTickTime;
    
    public bool OnManualTextInputDown(Key key, double time) {
        if (key != _lastKeyDown) {
            _lastKeyDown = key;
            _nextTickTime = time + InitDelay;
            return true;
        }

        if (time < _nextTickTime) {
            return false;
        }

        _nextTickTime = time + RepeatDelay;
        return true;
    }
    
    public void OnManualTextInputUp(Key key) {
        if (key != _lastKeyDown) {
            return;
        }

        _lastKeyDown = Key.Unknown;
    }
}