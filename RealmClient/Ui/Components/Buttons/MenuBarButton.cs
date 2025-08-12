using System;
using Common;
using RealmClient.UiLib.BuiltIn.Buttons;
using OpenTK.Mathematics;

namespace RealmClient.Ui.Components.Buttons;

public sealed class MenuBarButton : TextButton {
    
    private readonly bool _pulse;

    public MenuBarButton(string text, float size, Action callback, bool pulse = false) : base (new TextButtonConfig { Text = text, FontSize = size, OnClicked = callback, OutlineThickness = 4 }) {
        _pulse = pulse;
    }

    public MenuBarButton(TextButtonConfig config, bool pulse = false) : base(config) {
        _pulse = pulse;
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (!_pulse) return;
        
        var scale = 1.05f + 0.05f * (float)Math.Sin(gameTime.TotalMs / 200);
        Scale = new Vector2(scale);
    }
}