using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AlloyClient.UiLib.Core;
using Common;

namespace AlloyClient.UiLib.Extra;

public static class GTween {
    
    private static readonly Dictionary<Easing, Func<double, double>> Easings = new();
    private static readonly List<Tween> Tweens = [];

    static GTween() {
        Easings[Easing.Linear] = r => r;
        Easings[Easing.SineInOut] = r => -0.5f * (Math.Cos(r * Math.PI) - 1);
        Easings[Easing.BackInOut] = r => (r *= 2) < 1 ? 0.5f * (r * r * (3.59490f * r - 2.59490f)) : 0.5f * ((r -= 2) * r * (3.59490f * r + 2.59490f) + 2);
    }

    public static void Add(Tween tween) => Tweens.Add(tween);

    public static void Update(GameTime gameTime) {
        var count = Tweens.Count;

        if (count == 0) return;

        for (var i = 0; i < count; i++) {
            ref var tween = ref CollectionsMarshal.AsSpan(Tweens)[i];
            var ratio = Easings[tween.Ease](tween.TotalDt / tween.DurationMs);

            if (tween.DeltaDelay < tween.DelayMs) {
                tween.DeltaDelay += gameTime.ElapsedMs;

                if (tween.DeltaDelay >= tween.DelayMs)
                    tween.SetStart();
                continue;
            }

            if (tween.TotalDt >= tween.DurationMs) {
                tween.Sprite.TweenActive = false;
                tween.Finished = true;
                tween.OnFinish?.Invoke();
            }


            tween.TotalDt += gameTime.ElapsedMs;

            var value = (tween.End - tween.Start) * ratio + tween.Start;

            if (tween.Finished)
                value = tween.End;

            switch (tween.Type) {
                case EaseType.X:
                    tween.Sprite.X = (int)value;
                    continue;
                case EaseType.Y:
                    tween.Sprite.Y = (int)value;
                    continue;
                case EaseType.Alpha:
                    tween.Sprite.Alpha = (float)value;
                    continue;
            }
        }

        Tweens.RemoveAll(tween => tween.Finished);
    }
}

public struct Tween {
    public Easing Ease;

    public double TotalDt;
    public float DurationMs;
    public double DeltaDelay;
    public int DelayMs;

    public bool Finished;

    public Sprite Sprite;

    public float Start;
    public float End;
    public EaseType Type;
    public Action OnFinish;

    public static Tween New(Sprite sprite, Easing ease, int durationMs, float end, EaseType type, int delayMs = 0, Action onFinish = null) {
        if (delayMs == 0)
            sprite.TweenActive = true;
        
        return new Tween {
            Sprite = sprite,
            Ease = ease,
            DurationMs = durationMs,
            DelayMs = delayMs,
            Start = GetStart(sprite, type),
            End = end,
            Type = type,
            OnFinish = onFinish
        };
    }

    public void SetStart() {
        Start = GetStart(Sprite, Type);
        Sprite.TweenActive = true;
    }

    private static float GetStart(Sprite sprite, EaseType type) {
        return type switch {
            EaseType.X => sprite.X,
            EaseType.Y => sprite.Y,
            EaseType.Alpha => sprite.Alpha,
            _ => throw new Exception()
        };
    }
}

public enum EaseType {
    X,
    Y,
    Alpha
}

public enum Easing {
    Linear,
    SineInOut,
    BackInOut
}