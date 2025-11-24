using System;
using AlloyClient.Game.Components.Options.Ui;
using AlloyClient.State.SettingTypes;
using AlloyClient.Utils;
using Common;

namespace AlloyClient.Game.Components.Options.OptionTypes;

public class ChoiceOption<T> : Option {
    private static readonly Logger Log = new(typeof(ChoiceBox<T>));

    private readonly ChoiceBox<T> _choiceBox;
    private readonly Action _choiceCallback;

    public ChoiceOption(ValueSetting<T> setting, string[] labels, object[] values, string desc, string tooltipDesc, Action choiceCallback = null)
        : base(setting, desc, tooltipDesc) {
        if (labels.Length != values.Length) {
            Log.Error($"Wrong option setup for {setting} (Labels:{labels.Length}, Values:{values.Length})");
        }

        _choiceBox = new ChoiceBox<T>(setting, labels, values, OnChoiceChange);
        AddChild(_choiceBox);

        _choiceCallback = choiceCallback;

        //todo:SetBaseDimensions(_choiceBox.Width + DescText.X + DescText.Width, _choiceBox.Height);
    }

    public override void Refresh() {
        Console.WriteLine("Choice refresh");
    }

    public override void SetDisabled(bool val) {
        _disabled = val;
        // TODO: SetDisabled for ChoiceOption
    }

    private void OnChoiceChange() {
        _choiceCallback?.Invoke();
    }
}