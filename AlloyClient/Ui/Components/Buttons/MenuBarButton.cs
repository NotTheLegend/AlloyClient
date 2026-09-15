using System;
using Alloy.UiLib.Core;
using Alloy.Common;
using OpenTK.Mathematics;

namespace AlloyClient.Ui.Components.Buttons;

public sealed class MenuBarButton : TextButton {
    
    private readonly bool _pulse;

    public MenuBarButton(string text, float size, Action callback, bool pulse = false) : base (new TextButtonConfig { Text = text, FontSize = size, OnClicked = callback, OutlineThickness = 4 }) {
        _pulse = pulse;
        AddEventListener(Event.AddedToStage, AddedToStage);
        AddEventListener(Event.RemovedFromStage, RemovedFromStage);
    }

    public MenuBarButton(TextButtonConfig config, bool pulse = false) : base(config) {
        _pulse = pulse;
        
        AddEventListener(Event.AddedToStage, AddedToStage);
        AddEventListener(Event.RemovedFromStage, RemovedFromStage);
    }

    private void AddedToStage() {
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void RemovedFromStage() {
        RemoveEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void OnFrameEnter() {
        if (!_pulse) return;

        var gameTime = Stage.GameTime;
        var scale = 1.05f + 0.05f * (float)Math.Sin(gameTime.TotalMs / 200);
        Scale = new Vector2(scale);
    }
}