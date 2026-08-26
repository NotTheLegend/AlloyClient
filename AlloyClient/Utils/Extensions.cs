using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;

namespace AlloyClient.Utils;

public static class Extensions {
    
    extension(Random random) {
        public int NextRange(int max) => random.Next(max + 1);
        
        public int NextRange(int min, int max) => random.Next(min, max + 1);
        
        public float PlusMinus(float range) => random.NextSingle() * range * 2 - range;
    }
    
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

    extension<T>(List<T> collection) {
        public ReadOnlySpan<T> AsReadOnlySpan() => CollectionsMarshal.AsSpan(collection);
    }
}