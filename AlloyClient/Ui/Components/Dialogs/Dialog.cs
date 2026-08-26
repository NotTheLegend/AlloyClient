using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Ui.Components.Buttons;

using System.Collections.Generic;

namespace AlloyClient.Ui.Components.Dialogs;

public sealed record DialogOption(string Text, Action Callback = null);

public enum DialogState {
    Active = 0,
    Closed = 1,
    Finished = 2
}

public class Dialog : UiElement {

    private const int BoxWidth = 300;
    private const int BoxPadding = 16;
    private const int TopPadding = 10;
    private const int TextGap = 14;
    private const int MessageLineGap = 2;
    private const int ActionGap = 18;
    private const int BottomPadding = 10;

    public DialogState State = DialogState.Active;

    public Dialog(string title, string message, DialogOption confirm, DialogOption cancel = null) {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetAnchor(UiAnchor.Middle);

        var titleText = new SimpleText(new TextConfig {
            Text = title,
            FontSize = 20,
            FontType = FontType.Bold,
            Color = 0x78D67A,
            MaxWidth = BoxWidth - BoxPadding * 2,
            Anchor = UiAnchor.MiddleTop
        });

        var messageLines = new List<SimpleText>();
        if (!string.IsNullOrWhiteSpace(message)) {
            foreach (var line in WrapMessage(message, BoxWidth - BoxPadding * 2, 18)) {
                messageLines.Add(new SimpleText(new TextConfig {
                    Text = line,
                    FontSize = 18,
                    Color = 0xD0D0D0,
                    Anchor = UiAnchor.MiddleTop
                }));
            }
        }

        var confirmButton = new TextButton(new TextButtonConfig {
            Text = confirm.Text,
            FontSize = 22,
            FontType = FontType.Bold,
            OnClicked = () => {
                confirm.Callback?.Invoke();
                State = DialogState.Closed;
            },
            Anchor = UiAnchor.MiddleBottom
        });

        TextButton cancelButton = null;
        if (cancel != null) {
            cancelButton = new TextButton(new TextButtonConfig {
                Text = cancel.Text,
                FontSize = 22,
                FontType = FontType.Bold,
                OnClicked = () => {
                    cancel.Callback?.Invoke();
                    State = DialogState.Closed;
                },
                Anchor = UiAnchor.MiddleBottom
            });
        }

        var contentY = TopPadding + titleText.Height;
        if (messageLines.Count > 0) {
            contentY += TextGap;
            foreach (var line in messageLines) {
                contentY += line.Height;
            }
            contentY += (messageLines.Count - 1) * MessageLineGap;
        }

        var actionHeight = Math.Max(confirmButton.Height, cancelButton?.Height ?? 0);
        var boxHeight = contentY + ActionGap + actionHeight + BottomPadding;
        var panel = new Container(new ContainerConfig {
            Width = BoxWidth,
            Height = boxHeight
        });
        AddChild(panel);

        panel.AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = BoxWidth,
            Height = boxHeight,
            CutX = 7,
            CutY = 7,
            Color = 0xE6E6E6,
            Alpha = 0.9f
        }));
        panel.AddChild(new CutEdgeRect(new CutEdgeConfig {
            X = 1,
            Y = 1,
            Width = BoxWidth - 2,
            Height = boxHeight - 2,
            CutX = 6,
            CutY = 6,
            Color = 0x303030,
            Alpha = 0.98f
        }));
        titleText.X = BoxWidth / 2;
        titleText.Y = TopPadding;
        panel.AddChild(titleText);

        var messageY = TopPadding + titleText.Height + TextGap;
        foreach (var line in messageLines) {
            line.X = BoxWidth / 2;
            line.Y = messageY;
            panel.AddChild(line);
            messageY += line.Height + MessageLineGap;
        }

        var actionY = boxHeight - BottomPadding;
        confirmButton.X = cancelButton == null ? BoxWidth / 2 : BoxWidth * 3 / 4;
        confirmButton.Y = actionY;
        panel.AddChild(confirmButton);

        if (cancelButton != null) {
            cancelButton.X = BoxWidth / 4;
            cancelButton.Y = actionY;
            panel.AddChild(cancelButton);
        }
    }

    private static List<string> WrapMessage(string message, int maxWidth, int fontSize) {
        var lines = new List<string>();
        foreach (var paragraph in message.Split('\n')) {
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var current = string.Empty;
            foreach (var word in words) {
                var candidate = string.IsNullOrEmpty(current) ? word : $"{current} {word}";
                var measured = new SimpleText(new TextConfig {
                    Text = candidate,
                    FontSize = fontSize
                });

                if (measured.Width > maxWidth && !string.IsNullOrEmpty(current)) {
                    lines.Add(current);
                    current = word;
                } else {
                    current = candidate;
                }
            }

            if (!string.IsNullOrEmpty(current)) {
                lines.Add(current);
            }
        }

        return lines;
    }

    protected override void OnResize(ResizeEvent args) {
        Scale = Stage.ScreenScale;
        X = args.Width / 2;
        Y = args.Height / 2;
    }
}
