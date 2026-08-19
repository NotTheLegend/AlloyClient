using System;
using Alloy.UiLib.Core;
using Alloy.Common;
using OpenTK.Mathematics;

namespace AlloyClient.Ui.Components.Buttons;

public sealed class MenuBarButton : TextButton {

    public MenuBarButton(string text, float size, Action callback, bool pulse = false) : base (new TextButtonConfig { Text = text, FontSize = size, OnClicked = callback, OutlineThickness = 4 }) {
        AddPulse(pulse);
    }

    public MenuBarButton(TextButtonConfig config, bool pulse = false) : base(config) {
        AddPulse(pulse);
    }

    private void AddPulse(bool pulse) {
        if (!pulse) {
            return;
        }
        
        AddEventListener(Event.AddedToStage, () => AddEventListener(Event.EnterFrame, OnFrameEnter));
        RemoveEventListener(Event.AddedToStage, () => RemoveEventListener(Event.EnterFrame, OnFrameEnter));
    }

    private void OnFrameEnter() {
        var gameTime = Stage.GameTime;
        var scale = 1.05f + 0.05f * (float)Math.Sin(gameTime.TotalMs / 200);
        Scale = new Vector2(scale);
    }
}