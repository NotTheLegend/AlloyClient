using RealmClient.Game.Components.Options.Ui;
using RealmClient.State.SettingTypes;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;

namespace RealmClient.Game.Components.Options.OptionTypes;

public abstract class Option : Sprite {
    // TODO: tooltip
    public readonly ISettingType Setting;
    protected readonly SimpleText DescText;
    protected bool _disabled;

    protected Option(ISettingType setting, string desc, string tooltipDesc) {
        Setting = setting;
        
        if (!string.IsNullOrEmpty(desc)) {
            DescText = new SimpleText(new TextConfig {
                Text = desc,
                FontSize = 25,
                OutlineThickness = 2,
                Color = 0xB3B3B3,
                X = KeyCodeBox.BoxWidth + 38,
                Y = (KeyCodeBox.BoxHeight - 25) / 2,
            });
            AddChild(DescText);
        }

        if (!string.IsNullOrEmpty(tooltipDesc)) {
            // Add tooltip here
        }
    }

    public abstract void Refresh();

    public abstract void SetDisabled(bool val);
}