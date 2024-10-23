using Microsoft.Xna.Framework.Input;

namespace MonoClient.Utils {
    internal static class MonoUtils {
        public static bool AreKeysDown(this KeyboardState keyState, params Keys[] keys) {
            foreach (var key in keys) {
                if (!keyState.IsKeyDown(key)) {
                    return false;
                }
            }
            return true;
        }

        public static bool AnyKeysDown(this KeyboardState keyState, params Keys[] keys) {
            foreach (var key in keys) {
                if (keyState.IsKeyDown(key)) {
                    return true;
                }
            }
            return false;
        }

        public static bool AreKeysUp(this KeyboardState keyState, params Keys[] keys) {
            foreach (var key in keys) {
                if (!keyState.IsKeyUp(key)) {
                    return false;
                }
            }
            return true;
        }

        public static bool AnyKeysUp(this KeyboardState keyState, params Keys[] keys) {
            foreach (var key in keys) {
                if (keyState.IsKeyUp(key)) {
                    return true;
                }
            }
            return false;
        }
    }
}
