using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Ui.Components.Panels;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;

namespace MonoClient.Display;

public sealed class OverlayManager : Sprite {
    
    private enum OverlayState {
        None,
        Active,
        Closed,
        Finished
    }

    private static readonly Sprite Overlay = new ColorRect(new ColorRectConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x2B2B2B, Alpha = 0.8f });
    
    private static readonly Queue<Overlay> Overlays = [];
    private static Overlay _current;
    private static OverlayState _state = OverlayState.None;
    
    public static void Enqueue(Overlay overlay) {
        Overlays.Enqueue(overlay);
    }

    public static void CloseOverlay(Overlay overlay) {
        if (overlay != _current) return;
        _state = OverlayState.Closed;
    }

    public static void CloseOverlay()
    {
        if (_current != null)
        {
            _current.ClosePanel();
        }
    }

    public static bool CurrentOverlayIs(Overlay overlay)
        => _current != null && _current == overlay;

    protected override void OnUpdate(GameTime gameTime) {
        if (_state == OverlayState.None && !TryStart(true)) return;
        if (_state == OverlayState.Closed) OnClosed();
    }

    private bool TryStart(bool dimTween) {
        if (!Overlays.TryDequeue(out var panel)) return false;

        _current = panel;
        _state = OverlayState.Active;
        InputHandler.AddInputBlocker(InputBlockers.Panel);

        if (dimTween) {
            AddChild(Overlay);
            Overlay.AddAlphaTween(0f, 0.8f, 250);
        }
        
        _current.Alpha = 0f;
        AddChild(_current);
        GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 1f, EaseType.Alpha));
        return true;
    }

    private void OnClosed() {
        _state = OverlayState.Finished;

        if (Overlays.TryPeek(out _)) {
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => {
                RemoveChild(_current);
                TryStart(false);
            }));
        } else {
            GTween.Add(Tween.New(Overlay, Easing.SineInOut, 250, 0f, EaseType.Alpha));
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => {
                RemoveChild(_current);
                _current = null;
                _state = OverlayState.None;
                InputHandler.RemoveInputBlocker(InputBlockers.Panel);
            }));
        }
    }
    
}