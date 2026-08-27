using System;
using System.Collections.Generic;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Panels;
using OpenTK.Platform;

namespace AlloyClient.Screens.Components.Containers.Account;

public abstract class AccountFrame : Overlay {
    private const int FrameWidth = 475;
    private const int FieldX = 28;
    private const int FieldWidth = FrameWidth - FieldX * 2;

    private const uint TitleColor = 0xB3B3B3;
    private const uint ErrorColor = 0xFC4242;

    private readonly SimpleText _title;
    private SimpleText _status;

    private readonly List<AccountFormField> _fields = [];
    private readonly List<TextButton> _buttons = [];

    private bool _fieldsEnabled = true;

    protected AccountFrame(string title, int height) {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetAnchor(UiAnchor.Middle);

        AddChild(new CutEdgeRect(new CutEdgeConfig {
            Width = FrameWidth,
            Height = height,
            CutX = 8,
            CutY = 8,
            Color = 0xFFFFFF
        }));

        AddChild(new CutEdgeRect(new CutEdgeConfig {
            X = 1,
            Y = 1,
            Width = FrameWidth - 2,
            Height = height - 2,
            CutX = 7,
            CutY = 7,
            Color = 0x363636
        }));

        AddChild(new CutEdgeRect(new CutEdgeConfig {
            X = 1,
            Y = 1,
            Width = FrameWidth - 2,
            Height = 50,
            CutX = 7,
            CutY = 7,
            Cuts = CutEdges.Top,
            Color = 0x4D4D4D
        }));

        _title = new SimpleText(new TextConfig {
            Text = title,
            FontSize = 20,
            FontType = FontType.Bold,
            X = 12,
            Y = 14,
            MaxWidth = FrameWidth - 24,
            Color = TitleColor,
            OutlineThickness = 2
        });

        AddChild(_title);

        AddEventListener(Event.RemovedFromStage, OnRemovedFromStage);
    }

    protected AccountFormField AddField(string label, int y, bool password = false, byte maxCharacters = byte.MaxValue) {
        var field = new AccountFormField(label, FieldWidth, password, maxCharacters, ClearStatus) {
            X = FieldX,
            Y = y
        };

        AddChild(field);
        _fields.Add(field);
        field.SetTabIndex(_fields.Count - 1);
        return field;
    }

    protected void AddActions(string leftText, Action leftAction, string rightText, Action rightAction) {
        var rightButton = new TextButton(new TextButtonConfig {
            Text = rightText,
            FontSize = 28,
            FontType = FontType.Normal,
            OnClicked = rightAction,
            X = Width - 25,
            Y = Height - 52,
            Anchor = UiAnchor.RightTop
        });

        AddChild(rightButton);
        _buttons.Add(rightButton);

        var leftButton = new TextButton(new TextButtonConfig {
            Text = leftText,
            FontSize = 28,
            FontType = FontType.Normal,
            OnClicked = leftAction,
            X = rightButton.X - rightButton.Width - 35,
            Y = rightButton.Y,
            Anchor = UiAnchor.RightTop
        });

        AddChild(leftButton);
        _buttons.Add(leftButton);
    }

    protected void AddNavigation(string text, int y, Action onClicked) {
        var link = new TextButton(new TextButtonConfig {
            Text = text,
            FontSize = 16,
            FontType = FontType.Bold,
            OnClicked = onClicked,
            X = FieldX,
            Y = y
        });

        AddChild(link);
        _buttons.Add(link);

        _status = new SimpleText(new TextConfig {
            Text = string.Empty,
            FontSize = 14,
            X = FieldX,
            Y = y + link.Height + 6,
            MaxWidth = FieldWidth,
            Color = ErrorColor
        });

        AddChild(_status);
    }

    protected void SetStatus(string message) {
        _status?.SetText(message);
    }

    protected void ClearStatus() {
        _status?.SetText(string.Empty);
    }

    protected void SetActionsEnabled(bool enabled) {
        _fieldsEnabled = enabled;

        foreach (var button in _buttons) {
            button.SetState(enabled);
        }

        foreach (var field in _fields) {
            field.SetEnabled(enabled);
        }
    }

    private void OnRemovedFromStage() {
        _fieldsEnabled = false;

        foreach (var field in _fields) {
            field.SetEnabled(false);
        }
    }

}

public sealed class AccountFormField : Sprite {
    private const uint LabelColor = 0xB3B3B3;
    private const uint ErrorColor = 0xFC4242;

    private readonly SimpleText _error;
    private readonly TextInput _input;
    private readonly Action _onChange;

    private bool _focused;

    public string Text => _input.Text;
    public bool Focused => _focused;

    public AccountFormField(string label, int width, bool password, byte maxCharacters, Action onChange) {
        _onChange = onChange;

        AddChild(new SimpleText(new TextConfig {
            Text = label,
            FontSize = 20,
            FontType = FontType.Bold,
            Color = LabelColor
        }));

        _input = new TextInput(new InputConfig {
            Y = 27,
            Width = width,
            FontSize = 22,
            FontType = FontType.Normal,
            DefaultText = string.Empty,
            MaxCharacters = maxCharacters,
            Password = password,
            OnChange = OnInputChanged,
            OnFocus = () => _focused = true,
            OnUnfocus = () => _focused = false
        });

        AddChild(_input);

        _error = new SimpleText(new TextConfig {
            Text = string.Empty,
            FontSize = 14,
            Y = 61,
            Color = ErrorColor
        });

        AddChild(_error);
    }

    public bool HasText() => _input.HasText(true);

    public void Focus() => _input.Focus();

    public void SetTabIndex(int tabIndex) {
        _input.TabIndex = tabIndex;
    }

    public void SetError(string error) => _error.SetText(error);

    public void ClearError() => _error.SetText(string.Empty);

    public void SetEnabled(bool enabled) {
        _input.MouseEnabled = enabled;
        _input.FocusEnabled = enabled;
        _input.TabEnabled = enabled;
        if (!enabled) {
            _input.UnFocus();
        }
    }

    private void OnInputChanged() {
        ClearError();
        _onChange?.Invoke();
    }
}
