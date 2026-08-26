using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Graphics;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorPrompt : Container {
    public readonly TextInput[] Inputs;
    private readonly Action<string[]> _accepted;
    private readonly Action _closed;
    private readonly Container _panel;
    private readonly int _panelWidth;
    private readonly int _panelHeight;

    public EditorPrompt(string title, string[] labels, string[] values, Action<string[]> accepted, Action closed)
        : base(new ContainerConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight }) {
        _accepted = accepted;
        _closed = closed;

        _panelWidth = 360;
        _panelHeight = 95 + labels.Length * 42;
        _panel = new Container(new ContainerConfig {
            X = 0, Y = 0,
            Width = _panelWidth, Height = _panelHeight, Anchor = UiAnchor.Middle,
        });

        _panel.AddChild(new ColorRect(new ColorRectConfig
            { Width = _panelWidth, Height = _panelHeight, Color = 0x303030, MouseEnabled = true }));

        _panel.AddChild(new CutCornerOutline(_panelWidth, _panelHeight, 0xFFFFFF, 0.65f));
        _panel.AddChild(new SimpleText(new TextConfig {
            Text = title, FontSize = 24, FontType = FontType.Bold, X = 16, Y = 14,
        }));

        Inputs = new TextInput[labels.Length];
        for (var i = 0; i < labels.Length; i++) {
            var y = 52 + i * 42;
            var inputWidth = labels[i] is "Width" or "Height" ? 82 : 212;
            _panel.AddChild(new SimpleText(new TextConfig {
                Text = labels[i], FontSize = 16, X = 18, Y = y, Anchor = UiAnchor.LeftTop,
            }));

            Inputs[i] = new TextInput(new InputConfig {
                X = 130, Y = y - 4, Width = inputWidth, FontSize = 18, DefaultText = values[i], MaxCharacters = 80,
            });

            _panel.AddChild(Inputs[i]);
        }

        _panel.AddChild(new TextButton(new TextButtonConfig {
            Text = "Cancel", FontSize = 22, X = _panelWidth / 2 - 52, Y = _panelHeight - 22,
            Anchor = UiAnchor.Middle, OnClicked = Close,
        }));

        _panel.AddChild(new TextButton(new TextButtonConfig {
            Text = "Ok", FontSize = 22, X = _panelWidth / 2 + 52, Y = _panelHeight - 22,
            Anchor = UiAnchor.Middle, OnClicked = Accept,
        }));

        AddChild(_panel);
        AddEventListener(Event.AddedToStage, OnAddedToStage);
        AddEventListener(Event.RemovedFromStage, OnRemovedFromStage);
        if (Inputs.Length > 0) AddEventListener(Event.AddedToStage, Inputs[0].Focus);
    }

    private void OnAddedToStage() {
        Stage.AddEventListener(ResizeEvent.Resize, OnResize);
        OnResize(new ResizeEvent(ResizeEvent.Resize, Stage.StageWidth, Stage.StageHeight));
    }

    private void OnRemovedFromStage() => Stage.RemoveEventListener(ResizeEvent.Resize, OnResize);

    private void OnResize(ResizeEvent args) {
        Scale = Stage.ScreenScale;
        X = args.Width / 2;
        Y = args.Height / 2;
    }

    private void Accept() {
        var values = new string[Inputs.Length];
        for (var i = 0; i < Inputs.Length; i++) values[i] = Inputs[i].Text;
        _accepted(values);
        _closed();
    }

    private void Close() => _closed();
}