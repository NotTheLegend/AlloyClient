using Microsoft.Xna.Framework.Input;

namespace MonoClient.UiLib.Input;

public static class KeyboardInput {

    private static KeyboardState _prevState;
    private static KeyboardState _currState;

    public static void Update() {
        _prevState = _currState;
        _currState = Keyboard.GetState();
    }

    public static bool IsKeyDown(Keys key) {
        return _currState.IsKeyDown(key);
    }

    public static bool IsKeyUp(Keys key) {
        return _currState.IsKeyUp(key);
    }

    public static bool AreKeysDown(params Keys[] keys) {
        return _currState.AreKeysDown(keys);
    }

    public static bool AreKeysUp(params Keys[] keys) {
        return _currState.AreKeysUp(keys);
    }

    public static bool IsKeyPressed(Keys key) {
        return _prevState.IsKeyDown(key) && _currState.IsKeyUp(key);
    }

    public static bool AreKeysPressed(params Keys[] keys) {
        return _prevState.AreKeysDown(keys) && _currState.AnyKeysUp(keys);
    }
}