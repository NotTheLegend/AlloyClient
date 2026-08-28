using AlloyClient.Game.Components.Options.Ui;
using OpenTK.Platform;

namespace AlloyClient.Game.Components.Options.OptionTypes;

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
        _keyCodeBox.SetDisabled(val);
        DescText?.SetColor(val ? 0x666666u : 0xB3B3B3u);
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
