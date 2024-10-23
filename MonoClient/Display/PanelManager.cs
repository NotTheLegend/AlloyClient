using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.State;
using MonoClient.Ui.Components.Panels;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;

namespace MonoClient.Display;

public static class PanelManager {

    private static DimOverlay _overlay = new();
    
    private static Queue<Panel> _panels = [];
    private static Panel _current = null;

    public static void Enqueue(Panel panel) {
        _panels.Enqueue(panel);
    }

    public static void Update(GameTime gameTime, ref DisplayState state) {
        if (_current == null && !TryStart(true)) return;
        
        if (_current!.State == PanelState.Closed) OnClosed();
        
        _overlay.Update(gameTime);
        _current!.Update(gameTime);
        
        
        //Console.WriteLine($"{_current.Width}, {_current.X}");
        
        state = DisplayState.Panel;
    }
    
    public static void HandleMouseEvents(ref MouseEventId consumed) {
        _current?.HandleMouseEvents(ref consumed);
    }

    public static void Draw(GameTime gameTime) {
        if (_current == null) return;
        
        _overlay.Draw(gameTime);
        _current.Draw(gameTime);
    }

    private static bool TryStart(bool overlayTween) {
        if (!_panels.TryDequeue(out var panel)) return false;

        _current = panel;

        if (overlayTween) {
            _overlay.Alpha = 0f;
            GTween.Add(Tween.New(_overlay, Easing.SineInOut, 250, 1f, EaseType.Alpha));
        }
        
        _current.Alpha = 0f;
        GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 1f, EaseType.Alpha));
        return true;
    }

    private static void OnClosed() {
        _current.State = PanelState.Finished;

        if (_panels.TryPeek(out _)) {
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => TryStart(false)));
        } else {
            GTween.Add(Tween.New(_overlay, Easing.SineInOut, 250, 0f, EaseType.Alpha));
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => _current = null));
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