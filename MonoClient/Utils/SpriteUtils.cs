using System;
using MonoClient.UiLib;
using MonoClient.UiLib.Core;

namespace MonoClient.Utils;

public static class SpriteUtils {
    
    public static Sprite GetTypeFromList(this Sprite sprite, Type[] list) {
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

    public static void AddAlphaTween(this Sprite sprite, float start, float end, int duration, Easing easing = Easing.SineInOut, int delay = 0) {
        sprite.Alpha = start;
        GTween.Add(Tween.New(sprite, easing, duration, end, EaseType.Alpha, delay));
    }
}