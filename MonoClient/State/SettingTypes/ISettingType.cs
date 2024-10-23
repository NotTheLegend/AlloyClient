namespace MonoClient.State.SettingTypes;

public interface ISettingType {
    void SetValue(ISettingType newValue);
    string Serialize();
    void Deserialize(string str);
}