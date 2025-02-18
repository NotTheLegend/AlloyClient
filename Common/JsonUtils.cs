using System.Text.Json;

namespace Common;

public static class JsonUtils {

    public static float GetValueFloat(this JsonElement element, string name) => element.GetProperty(name).GetSingle();
    public static int GetValueInt(this JsonElement element, string name) => element.GetProperty(name).GetInt32();
}