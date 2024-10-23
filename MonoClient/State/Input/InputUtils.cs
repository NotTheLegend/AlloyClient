using Microsoft.Xna.Framework.Input;
using MonoClient.State.SettingTypes;

namespace MonoClient.State.Input;

public struct StateContainer {
    public KeyboardState KeyboardState;
    public MouseState MouseState;
}

public static class InputUtils {
    public static bool IsPressed(this StateContainer state, InputSetting setting) {
        if (setting.Key != Keys.None) {
            return state.KeyboardState.IsKeyDown(setting.Key);
        }

        if (setting.Mouse != MouseButton.None) {
            return setting.Mouse switch {
                MouseButton.Left => state.MouseState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => state.MouseState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => state.MouseState.MiddleButton == ButtonState.Pressed,
                MouseButton.XButton1 => state.MouseState.XButton1 == ButtonState.Pressed,
                MouseButton.XButton2 => state.MouseState.XButton2 == ButtonState.Pressed,
                _ => false
            };
        }

        return false;
    }

    public static bool IsPressed(this StateContainer state, Keys key) {
        return key != Keys.None && state.KeyboardState.IsKeyDown(key);
    }

    public static bool IsPressed(this StateContainer state, MouseButton button) {
        return button switch {
            MouseButton.Left => state.MouseState.LeftButton == ButtonState.Pressed,
            MouseButton.Right => state.MouseState.RightButton == ButtonState.Pressed,
            MouseButton.Middle => state.MouseState.MiddleButton == ButtonState.Pressed,
            MouseButton.XButton1 => state.MouseState.XButton1 == ButtonState.Pressed,
            MouseButton.XButton2 => state.MouseState.XButton2 == ButtonState.Pressed,
            _ => false
        };
    }

    public static bool IsToggled(this StateContainer keyboardState, InputSetting setting,
        ref StateContainer prevKeyboardState) {
        if (setting.Key != Keys.None) {
            return keyboardState.KeyboardState.IsKeyDown(setting.Key) &&
                   prevKeyboardState.KeyboardState.IsKeyUp(setting.Key);
        }

        if (setting.Mouse != MouseButton.None) {
            return setting.Mouse switch {
                MouseButton.Left => keyboardState.MouseState.LeftButton == ButtonState.Pressed &&
                                    prevKeyboardState.MouseState.LeftButton == ButtonState.Released,
                MouseButton.Right => keyboardState.MouseState.RightButton == ButtonState.Pressed &&
                                     prevKeyboardState.MouseState.RightButton == ButtonState.Released,
                MouseButton.Middle => keyboardState.MouseState.MiddleButton == ButtonState.Pressed &&
                                      prevKeyboardState.MouseState.MiddleButton == ButtonState.Released,
                MouseButton.XButton1 => keyboardState.MouseState.XButton1 == ButtonState.Pressed &&
                                        prevKeyboardState.MouseState.XButton1 == ButtonState.Released,
                MouseButton.XButton2 => keyboardState.MouseState.XButton2 == ButtonState.Pressed &&
                                        prevKeyboardState.MouseState.XButton2 == ButtonState.Released,
                _ => false
            };
        }

        return false;
    }
}