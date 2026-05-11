using System;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;

namespace AlloyClient.Utils;

public static class SpriteUtils {
    
    extension(Sprite sprite) {
        public Sprite GetTypeFromList(Type[] list) {
            var obj = sprite;
            var len = list.Length;

            while (obj != null) {
                for (var i = 0; i < len; i++) {
                    if (obj.GetType() == list[i])
                        return obj;
                }

                obj = obj.Parent;
            }
        
            return null;
        }

        public void AddAlphaTween(float start, float end, int duration, Easing easing = Easing.SineInOut, int delay = 0, Action onFinish = null) {
            sprite.Alpha = start;
            GTween.Add(Tween.New(sprite, easing, duration, end, EaseType.Alpha, delay, onFinish));
        }
    }
}