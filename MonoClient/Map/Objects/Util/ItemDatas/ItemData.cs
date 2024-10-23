using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MonoClient.Objects.Util.ItemDatas;

public abstract class ItemData {
    
    public static JsonSerializerSettings IgnoreDefaultsSetting = new() {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
    };
    
    public static ItemData ParseData(ItemData obj, IDictionary<string, object> data) {
        if (obj is null)
            return null;

        var type = obj.GetType();
        if (!type.IsSubclassOf(typeof(ItemData)))
            throw new ArgumentException($"{nameof(type)} does not inherit ItemData class.");

        var fields = type.GetFields();
        foreach (var field in fields)
            if (data.TryGetValue(field.Name, out var value)) {
                var fieldType = field.FieldType;
                if (fieldType == typeof(string)) {
                    field.SetValue(obj, value);
                }
                else if (fieldType == typeof(int)) {
                    if (int.TryParse(value.ToString(), out var res))
                        field.SetValue(obj, res);
                }
                else if (fieldType == typeof(ushort)) {
                    if (ushort.TryParse(value.ToString(), out var res))
                        field.SetValue(obj, res);
                }
                else if (fieldType == typeof(byte)) {
                    if (byte.TryParse(value.ToString(), out var res))
                        field.SetValue(obj, res);
                }
                else if (fieldType == typeof(double)) {
                    if (double.TryParse(value.ToString(), out var res))
                        field.SetValue(obj, res);
                }
                else if (fieldType == typeof(float)) {
                    if (float.TryParse(value.ToString(), out var res))
                        field.SetValue(obj, res);
                }
                else if (fieldType == typeof(bool)) {
                    if (bool.TryParse(value.ToString(), out var res))
                        field.SetValue(obj, res);
                }
                else if (fieldType == typeof(int[])) {
                    field.SetValue(obj, ((List<object>) value).Select(i => int.Parse(i.ToString())).ToArray());
                }
                else if (fieldType.IsSubclassOf(typeof(ItemData))) {
                    var dict = (IDictionary<string, object>) value;
                    field.SetValue(obj, ParseData((ItemData) field.GetValue(obj), dict));
                }
                else if (fieldType == typeof(ActivateEffectDesc[])) {
                    var jsons = ((List<object>) value).Select(i => JsonConvert.SerializeObject(i, IgnoreDefaultsSetting));
                    var array = jsons
                        .Select(json => JsonConvert.DeserializeObject<ActivateEffectDesc>(json, IgnoreDefaultsSetting)).ToArray();
                    field.SetValue(obj, array);
                }
                else if (fieldType == typeof(ActivateEffectDesc)) {
                    var json = JsonConvert.SerializeObject(value, IgnoreDefaultsSetting);
                    field.SetValue(obj, JsonConvert.DeserializeObject<ActivateEffectDesc>(json, IgnoreDefaultsSetting));
                }
            }
        
        var props = type.GetProperties();
        foreach (var prop in props)
            if (data.TryGetValue(prop.Name, out var value)) {
                var propType = prop.PropertyType;
                if (propType == typeof(string)) {
                    prop.SetValue(obj, value);
                }
                else if (propType == typeof(int)) {
                    if (int.TryParse(value.ToString(), out var res))
                        prop.SetValue(obj, res);
                }
                else if (propType == typeof(ushort)) {
                    if (ushort.TryParse(value.ToString(), out var res))
                        prop.SetValue(obj, res);
                }
                else if (propType == typeof(byte)) {
                    if (byte.TryParse(value.ToString(), out var res))
                        prop.SetValue(obj, res);
                }
                else if (propType == typeof(double)) {
                    if (double.TryParse(value.ToString(), out var res))
                        prop.SetValue(obj, res);
                }
                else if (propType == typeof(float)) {
                    if (float.TryParse(value.ToString(), out var res))
                        prop.SetValue(obj, res);
                }
                else if (propType == typeof(bool)) {
                    if (bool.TryParse(value.ToString(), out var res))
                        prop.SetValue(obj, res);
                }
                else if (propType == typeof(int[])) {
                    prop.SetValue(obj, ((List<object>) value).Select(i => int.Parse(i.ToString())).ToArray());
                }
                else if (propType.IsSubclassOf(typeof(ItemData))) {
                    var dict = (IDictionary<string, object>) value;
                    prop.SetValue(obj, ParseData((ItemData) prop.GetValue(obj), dict));
                }
                else if (propType == typeof(ActivateEffectDesc[])) {
                    var jsons = ((List<object>) value).Select(i => JsonConvert.SerializeObject(i, IgnoreDefaultsSetting));
                    var array = jsons
                        .Select(json => JsonConvert.DeserializeObject<ActivateEffectDesc>(json, IgnoreDefaultsSetting)).ToArray();
                    prop.SetValue(obj, array);
                }
                else if (propType == typeof(ActivateEffectDesc)) {
                    var json = JsonConvert.SerializeObject(value, IgnoreDefaultsSetting);
                    prop.SetValue(obj, JsonConvert.DeserializeObject<ActivateEffectDesc>(json, IgnoreDefaultsSetting));
                }
            }

        return obj;
    }
}