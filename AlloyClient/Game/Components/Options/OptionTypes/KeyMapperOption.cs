using OpenTK.Platform;
using RealmClient.Game.Components.Options.Ui;
using RealmClient.State;
using RealmClient.State.SettingTypes;

namespace RealmClient.Game.Components.Options.OptionTypes;

public class KeyMapperOption : Option {
    private readonly KeyCodeBox _keyCodeBox;

    public KeyMapperOption(InputSetting setting, string desc, string tooltipDesc) : base(setting, desc, tooltipDesc) {
        _keyCodeBox = new KeyCodeBox(setting, OnKeyCodeChange);
        AddChild(_keyCodeBox);

        //todo:SetBaseDimensions(_keyCodeBox.Width + DescText.X + DescText.Width, _keyCodeBox.Height);
    }

    public override void Refresh() {
        _keyCodeBox.SetValue(Setting as InputSetting);
    }
    
    public override void SetDisabled(bool val) {
        _disabled = val;
        // TODO: SetDisabled for KeyMapperOption
    }

    private void OnKeyCodeChange() {
        foreach (var inputSetting in Settings.Inputs) {
            if (inputSetting == Setting || inputSetting.Key != _keyCodeBox.Value.Key) {
                continue;
            }

            inputSetting.Set(Scancode.Unknown);
        }
        
        OptionsView.RefreshOptions.Dispatch();
    }
}