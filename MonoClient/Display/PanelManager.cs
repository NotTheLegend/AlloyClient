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

public sealed class PanelManager : Sprite {
    
    private enum PanelState {
        None,
        Active,
        Closed,
        Finished
    }

    private static readonly Sprite Overlay = new ColorRect(new ColorRectConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight, Color = 0x2B2B2B, Alpha = 0.8f });
    
    private static readonly Queue<Panel> Panels = [];
    private static Panel _current;
    private static PanelState _state = PanelState.None;
    
    public static void Enqueue(Panel panel) {
        Panels.Enqueue(panel);
    }

    public static void ClosePanel(Panel panel) {
        if (panel != _current) return;
        _state = PanelState.Closed;
    }

    public static void ClosePanel()
    {
        if (_current != null)
        {
            _current.ClosePanel();
        }
    }

    public static bool CurrentPanelIs(Panel panel)
        => _current != null && _current == panel;

    protected override void OnUpdate(GameTime gameTime) {
        if (_state == PanelState.None && !TryStart(true)) return;
        if (_state == PanelState.Closed) OnClosed();
    }

    private bool TryStart(bool dimTween) {
        if (!Panels.TryDequeue(out var panel)) return false;
        
        Console.WriteLine("EEenter");

        _current = panel;
        _state = PanelState.Active;
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
        _state = PanelState.Finished;

        if (Panels.TryPeek(out _)) {
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => {
                RemoveChild(_current);
                TryStart(false);
            }));
        } else {
            GTween.Add(Tween.New(Overlay, Easing.SineInOut, 250, 0f, EaseType.Alpha));
            GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => {
                RemoveChild(_current);
                _current = null;
                _state = PanelState.None;
                InputHandler.RemoveInputBlocker(InputBlockers.Panel);
            }));
        }
    }
    
}