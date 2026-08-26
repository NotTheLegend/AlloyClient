using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Graphics;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorPrompt : Container {
    private readonly TextInput[] _inputs;
    private readonly Action<string[]> _accepted;
    private readonly Action _closed;

    public EditorPrompt(string title, string[] labels, string[] values, Action<string[]> accepted, Action closed)
        : base(new ContainerConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight }) {
        _accepted = accepted;
        _closed = closed;

        const int panelWidth = 360;
        var panelHeight = 95 + labels.Length * 42;
        var panel = new Container(new ContainerConfig {
            X = 0, Y = 0,
            Width = panelWidth, Height = panelHeight, Anchor = UiAnchor.Middle,
        });

        panel.AddChild(new ColorRect(new ColorRectConfig
            { Width = panelWidth, Height = panelHeight, Color = 0x303030, MouseEnabled = true }));

        panel.AddChild(new CutCornerOutline(panelWidth, panelHeight, 0xFFFFFF, 0.65f));
        panel.AddChild(new SimpleText(new TextConfig {
            Text = title, FontSize = 24, FontType = FontType.Bold, X = 16, Y = 14,
        }));

        _inputs = new TextInput[labels.Length];
        for (var i = 0; i < labels.Length; i++) {
            var y = 52 + i * 42;
            var inputWidth = labels[i] is "Width" or "Height" ? 82 : 212;
            panel.AddChild(new SimpleText(new TextConfig {
                Text = labels[i], FontSize = 16, X = 18, Y = y, Anchor = UiAnchor.LeftTop,
            }));

            _inputs[i] = new TextInput(new InputConfig {
                X = 130, Y = y - 4, Width = inputWidth, FontSize = 18, DefaultText = values[i], MaxCharacters = 80,
            });

            panel.AddChild(_inputs[i]);
        }

        panel.AddChild(new TextButton(new TextButtonConfig {
            Text = "Cancel", FontSize = 22, X = panelWidth / 2 - 52, Y = panelHeight - 22,
            Anchor = UiAnchor.Middle, OnClicked = Close,
        }));

        panel.AddChild(new TextButton(new TextButtonConfig {
            Text = "Ok", FontSize = 22, X = panelWidth / 2 + 52, Y = panelHeight - 22,
            Anchor = UiAnchor.Middle, OnClicked = Accept,
        }));

        AddChild(panel);
        AddEventListener(Event.AddedToStage, OnAddedToStage);
        AddEventListener(Event.RemovedFromStage, OnRemovedFromStage);
        if (_inputs.Length > 0) {
            AddEventListener(Event.AddedToStage, _inputs[0].Focus);
        }
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
        var values = new string[_inputs.Length];
        for (var i = 0; i < _inputs.Length; i++) {
            values[i] = _inputs[i].Text;
        }
        
        _accepted(values);
        _closed();
    }

    private void Close() => _closed();
}