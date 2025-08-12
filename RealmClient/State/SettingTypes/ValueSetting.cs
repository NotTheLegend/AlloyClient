using System;

namespace RealmClient.State.SettingTypes;

public class ValueSetting<T> : ISettingType {
    public T Value;

    public static implicit operator T(ValueSetting<T> valueSetting) {
        return valueSetting.Value;
    }

    public static implicit operator ValueSetting<T>(T value) {
        return new ValueSetting<T> { Value = value };
    }

    public string Serialize() {
        if (typeof(T).IsEnum) {
            return Value.ToString();
        }

        return Value.ToString();
    }

    public void Deserialize(string str) {
        if (typeof(T).IsEnum) {
            Value = (T)Enum.Parse(typeof(T), str);
            return;
        }

        Value = (T)Convert.ChangeType(str, typeof(T));
    }

    public void SetValue(ISettingType newValue) {
        if (newValue is not ValueSetting<T> valueSetting) {
            return;
        }

        Value = valueSetting.Value;
    }

    public void SetValue(T value) {
        Value = value;
    }
}