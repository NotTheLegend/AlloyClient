using System;
using Alloy.UiLib;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;

namespace AlloyClient.Ui.Components.Buttons;

public sealed class MenuBarButton : TextButton {

    private const double PulsePeriodMs = 1800;
    private const float PulseAmplitude = 0.05f;

    public MenuBarButton(string text, float size, Action callback, bool pulse = false) : base (new TextButtonConfig { Text = text, FontSize = size, OnClicked = callback, OutlineThickness = 4 }) {
        AddLineBox(size, FontType.Bold);
        AddPulse(pulse);
    }

    public MenuBarButton(TextButtonConfig config, bool pulse = false) : base(config) {
        AddLineBox(config.FontSize, config.FontType);
        AddPulse(pulse);
    }

    private void AddLineBox(float size, FontType fontType) {
        var lineHeight = (int)Math.Ceiling(UiRender.GetFont(fontType).LineHeight * size);
        AddChild(new ColorRect(new ColorRectConfig {
            Width = 1,
            Height = lineHeight,
            Alpha = 0
        }));
    }

    private void AddPulse(bool pulse) {
        if (!pulse) {
            return;
        }
        
        AddEventListener(Event.AddedToStage, OnAddedToStage);
        AddEventListener(Event.RemovedFromStage, OnRemovedFromStage);
    }

    private void OnAddedToStage() {
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void OnRemovedFromStage() {
        RemoveEventListener(Event.EnterFrame, OnFrameEnter);
        Scale = Vector2.One;
    }

    private void OnFrameEnter() {
        var gameTime = Stage.GameTime;
        var phase = gameTime.TotalMs / PulsePeriodMs * Math.PI * 2;
        var scale = 1f + PulseAmplitude * (0.5f - 0.5f * (float)Math.Cos(phase));
        Scale = new Vector2(scale);
    }
}
