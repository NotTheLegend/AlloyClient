using System;
using AlloyClient.State;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Enums;
using AlloyClient.Ui.Components.Buttons;

namespace AlloyClient.Ui.Components.Dialogs;

public sealed record DialogOption(string Text, Action Callback = null);

public enum DialogState {
    Active = 0,
    Closed = 1,
    Finished = 2
}

public class Dialog : UiElement {

    private const int BoxWidth = 300;

    public DialogState State = DialogState.Active;

    public Dialog(string title, string message, DialogOption confirm, DialogOption cancel = null) {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetAnchor(UiAnchor.Middle);

        var boxConfig = new ColorRectConfig { Width = BoxWidth, Height = 75, Color = 0x1C1C1C, Alpha = 0.8f, Anchor = UiAnchor.LeftTop};
        var box = new ColorRect(boxConfig);
        AddChild(box);

        var titleConfig = new TextConfig { Text = title, FontSize = 24, FontType = FontType.Bold, Color = 0xFFFFFF, Anchor = UiAnchor.MiddleTop};
        var titleText = new SimpleText(titleConfig);
        AddChild(titleText);

        var messageConfig = new TextConfig {Text = message, FontSize = 20, Color = 0xFFFFFF, Anchor = UiAnchor.MiddleTop};
        var messageText = new SimpleText(messageConfig);
        box.AddChild(messageText);

        var confirmConfig = new TextButtonConfig { Text = confirm.Text, FontSize = 22, OnClicked = () => { confirm.Callback?.Invoke(); State = DialogState.Closed; }, Anchor = UiAnchor.MiddleBottom};
        var confirmButton = new TextButton(confirmConfig);
        box.AddChild(confirmButton);

        var boxHeight = titleText.Height + 10 + messageText.Height + 20 + confirmButton.Height + 10;
        box.Resize(BoxWidth, boxHeight);

        titleText.X = box.Width / 2;
        titleText.Y = 10;

        messageText.X = box.Width / 2;
        messageText.Y = titleText.Height + 20;

        confirmButton.Y = box.Height - 10;
        if (cancel != null) {
            confirmButton.X = 3 * box.Width / 4;

            var cancelConfig = new TextButtonConfig { Text = cancel.Text, FontSize = 22, OnClicked = () => {cancel.Callback?.Invoke(); State = DialogState.Closed; }, Anchor = UiAnchor.MiddleBottom };
            var cancelButton = new TextButton(cancelConfig);
            box.AddChild(cancelButton);

            cancelButton.Y = box.Height - 10;
            cancelButton.X = box.Width / 4;
        } else {
            confirmButton.X = box.Width / 2;
        }
    }

    protected override void OnResize(ResizeEvent args) {
        Scale = Stage.ScreenScale;
        X = args.Width / 2;
        Y = args.Height / 2;
    }
}