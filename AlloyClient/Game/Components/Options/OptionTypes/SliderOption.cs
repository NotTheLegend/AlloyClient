using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Enums;

namespace AlloyClient.Game.Components.Options.OptionTypes;

public class SliderOption : Option {
    private readonly SimpleText _text;

    public SliderOption(ValueSetting<float> setting, string text, Action<float> sliderCallback) : base(setting, null, null) {
        _text = new SimpleText(new TextConfig {
            Text = text,
            FontSize = 22,
            FontType = FontType.Bold,
            OutlineThickness = 2,
            Color = 0xABABAB
        });
        AddChild(_text);
    }

    public override void Refresh() {
    }
    
    public override void SetDisabled(bool val) {
        _disabled = val;
        // TODO: SetDisabled for SliderOption
    }
}