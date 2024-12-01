using System;
using MonoClient.UiLib.Core;

namespace MonoClient.Utils;

public static class Utils {
    
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
}