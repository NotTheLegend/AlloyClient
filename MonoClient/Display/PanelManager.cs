using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.State;
using MonoClient.Ui.Components.Panels;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Display;

public class PanelManager : Sprite {

    private static readonly DimOverlay Overlay = new();
    
    private static Queue<Panel> _panels = [];
    private static Panel _current = null;

    public static void Enqueue(Panel panel) {
        _panels.Enqueue(panel);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (_current == null && !TryStart(true)) return;
        if (_current!.State == PanelState.Closed) OnClosed();
    }

    private bool TryStart(bool overlayTween) {
        if (!_panels.TryDequeue(out var panel)) return false;

        _current = panel;

        if (overlayTween) {
            Overlay.Alpha = 0f;
            AddChild(Overlay);
            GTween.Add(Tween.New(Overlay, Easing.SineInOut, 250, 1f, EaseType.Alpha));
        }
        
        _current.Alpha = 0f;
        AddChild(_current);
        GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 1f, EaseType.Alpha));
        return true;
    }

    private void OnClosed() {
        _current.State = PanelState.Finished;

        if (_panels.TryPeek(out _)) {
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => {
                RemoveChild(_current);
                TryStart(false);
            }));
        } else {
            GTween.Add(Tween.New(Overlay, Easing.SineInOut, 250, 0f, EaseType.Alpha));
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => {
                RemoveChild(_current);
                _current = null;
            }));
        }
    }
    
}

public class DimOverlay : DisplayContainer {

    public DimOverlay() {
        var config = new ColorRectConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x2B2B2B, Alpha = 0.8f, Anchor = UiAnchor.LeftTop };
        var rect = new ColorRect(config);
        AddChild(rect);
    }
}