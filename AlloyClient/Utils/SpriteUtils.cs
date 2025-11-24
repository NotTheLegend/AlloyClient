using System;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Extra;
using AlloyClient.UiLib;

namespace AlloyClient.Utils;

public static class SpriteUtils {
    
    public static Sprite GetTypeFromList(this Sprite sprite, Type[] list) {
        var obj = sprite;
        var len = list.Length;

        while (obj != null) {
            for (var i = 0; i < len; i++) {
                if (obj.GetType() == list[i])
                    return obj;
            }

            obj = obj.Parent as Sprite;
        }
        
        return null;
    }

    public static void AddAlphaTween(this Sprite sprite, float start, float end, int duration, Easing easing = Easing.SineInOut, int delay = 0) {
        sprite.Alpha = start;
        GTween.Add(Tween.New(sprite, easing, duration, end, EaseType.Alpha, delay));
    }

    public static void SetAutoResize(this Sprite sprite, Action<ResizeEvent> callback) {
        sprite.AddEventListener(Event.AddedToStage, () => {
            sprite.AddEventListener(ResizeEvent.Resize, callback);
            callback(new ResizeEvent(ResizeEvent.Resize, sprite.Stage.StageWidth, sprite.Stage.StageHeight));
        });
        sprite.AddEventListener(Event.RemovedFromStage, () => {sprite.RemoveEventListener(ResizeEvent.Resize, callback);});
    }
}