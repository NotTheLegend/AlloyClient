using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoClient.Display;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Objects;
using MonoClient.Objects.Enums;
using MonoClient.Objects.Util;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.Screens.Game.Components.Hud.Chat;
using MonoClient.Screens.Game.Components.Options;
using MonoClient.UiLib.Utils.Signals;
using MonoClient.Ui.Components.Panels;
using MonoClient.Utils;

namespace MonoClient.State.Input;

public static class InputHandler {
    private static bool _autoFire;
    private static bool _uiBlockingInput = false;

    private static InputBlockers _inputBlockers = InputBlockers.None;

    private static StateContainer _prevInputState;
    private static StateContainer _currInputState;

    public static bool Reconnecting;
    public static bool Moving;

    public static readonly SingleSignal OnChatKey = new();
    public static readonly SingleSignal<string> OnChatOpen = new();

    //todo: add keyboard events to sprite and swap these to that system
    public static readonly SingleSignal OnChatHistoryUp = new();
    public static readonly SingleSignal OnChatHistoryDown = new();

    public static void AddInputBlocker(InputBlockers blocker) {
        _inputBlockers |= blocker;
    }

    public static void RemoveInputBlocker(InputBlockers blocker) {
        _inputBlockers &= ~blocker;
    }

    public static void Update(double time, double dt) {
        var state = new StateContainer {
            KeyboardState = Keyboard.GetState(),
            MouseState = Mouse.GetState()
        };

        _prevInputState = _currInputState;
        _currInputState = state;

        if (Map.LocalPlayer == null) return;

        HandleBlockedByEverythingInputs(state);
        HandleBlockedByPanelInputs(state);
        HandleBlockedByVisibleUIInputs(state);
        HandleUnblockedInputs(state);
    }

    /// <summary>
    /// Handle any inputs that are blocked by either a UI input block, or a panel being visible.
    /// </summary>
    /// <param name="state">Mouse and Keyboard state.</param>
    private static void HandleBlockedByEverythingInputs(StateContainer state) {
        if (_inputBlockers != InputBlockers.None) {
            Map.LocalPlayer.SetRelativeMovement(0, 0, 0);
            Map.LocalPlayer.AnimationType = AnimationType.Stand;
            return;
        }

        var moveUp = state.IsPressed(Settings.MoveUp);
        var moveDown = state.IsPressed(Settings.MoveDown);
        var moveLeft = state.IsPressed(Settings.MoveLeft);
        var moveRight = state.IsPressed(Settings.MoveRight);
        var rotateLeft = state.IsPressed(Settings.RotateLeft);
        var rotateRight = state.IsPressed(Settings.RotateRight);
        Map.LocalPlayer.SetRelativeMovement(
            (rotateRight ? 1 : 0) - (rotateLeft ? 1 : 0),
            (moveRight ? 1 : 0) - (moveLeft ? 1 : 0),
            (moveDown ? 1 : 0) - (moveUp ? 1 : 0)
        );

        Moving = moveUp || moveDown || moveLeft || moveRight;
        Map.LocalPlayer.AnimationType = Moving ? AnimationType.Walk : AnimationType.Stand;

        if (state.IsToggled(Settings.AutoFire, ref _prevInputState)) {
            _autoFire = !_autoFire;
        }

        if (_autoFire || state.IsPressed(Settings.Shoot)) {
            Map.LocalPlayer.IsShooting = true;

            var mousePosition = new Vector2(state.MouseState.X, state.MouseState.Y);
            var pos = Camera.ScreenToWorld(mousePosition, Main.GameInstance.GraphicsDevice.Viewport);

            float dX = pos.X - Map.LocalPlayer.Position.X;
            float dY = pos.Y - Map.LocalPlayer.Position.Y;

            Map.LocalPlayer.AttackAngle = MathF.Atan2(dY, dX);
            Map.LocalPlayer.Shoot(0f);
        }

        if (state.IsPressed(Settings.Special)) {
        }

        if (state.IsToggled(Settings.MenuButton, ref _prevInputState)) {
            //Client.Disconnect();
            return;
        }

        if (state.IsToggled(Settings.Escape, ref _prevInputState)) {
            if (Map.Name == "Nexus") {
                return;
            }

            if (!Reconnecting) {
                Client.QueuePacket(Escape.CreatePacket());
                Reconnecting = true;
                return;
            }
        }

        if (state.IsToggled(Settings.Interact, ref _prevInputState)) {
            if (Reconnecting) {
                return;
            }

            var entities = EntityUtils.FindEntitiesInRadius(Map.LocalPlayer, Map.Entities.Values, 2f);

            foreach (Entity en in entities) {
                if (en.Properties.Class == "Portal") {
                    Reconnecting = true;

                    var portal = UsePortal.CreatePacket();
                    portal.ObjectId = en.ObjectId;
                    Client.QueuePacket(portal);
                }
            }
        }

        if (state.IsPressed(Settings.Walk)) {
        }

        if (state.IsPressed(Settings.HealthPotion)) {
        }

        if (state.IsPressed(Settings.MagicPotion)) {
        }

        if (state.IsPressed(Settings.InvOne)) {
        }

        if (state.IsPressed(Settings.InvTwo)) {
        }

        if (state.IsPressed(Settings.InvThree)) {
        }

        if (state.IsPressed(Settings.InvFour)) {
        }

        if (state.IsPressed(Settings.InvFive)) {
        }

        if (state.IsPressed(Settings.InvSix)) {
        }

        if (state.IsPressed(Settings.InvSeven)) {
        }

        if (state.IsPressed(Settings.InvEight)) {
        }

        if (state.IsPressed(Settings.ResetCameraAngle)) {
            Settings.CameraAngle = 0;
        }

        Map.LocalPlayer.Focused = state.IsPressed(Settings.Focus);
    }

    /// <summary>
    /// Handle any inputs that are only blocked by a panel being visible.
    /// </summary>
    /// <param name="state">Mouse and Keyboard state.</param>
    private static void HandleBlockedByPanelInputs(StateContainer state) {
        if ((_inputBlockers & InputBlockers.Panel) != 0) return;

        if (state.IsToggled(Settings.Chat, ref _prevInputState)) {
            OnChatKey.Dispatch();
        }

        if (state.IsToggled(Settings.TellKey, ref _prevInputState)) {
            OnChatOpen.Dispatch("/tell ");
        }

        if (state.IsToggled(Settings.GuildChat, ref _prevInputState)) {
            OnChatOpen.Dispatch("/g ");
        }

        if (state.IsToggled(Settings.PartyChat, ref _prevInputState)) {
            OnChatOpen.Dispatch("/p ");
        }

        if (state.IsToggled(Settings.ChatHistoryUp, ref _prevInputState)) {
            OnChatHistoryUp.Dispatch();
        }

        if (state.IsToggled(Settings.ChatHistoryDown, ref _prevInputState)) {
            OnChatHistoryDown.Dispatch();
        }
    }

    /// <summary>
    /// Handle any keys that are only blocked by non-panel UI being visible.
    /// </summary>
    /// <param name="state">Mouse and Keyboard state.</param>
    private static void HandleBlockedByVisibleUIInputs(StateContainer state) {
        if ((_inputBlockers & (InputBlockers.Chat | InputBlockers.Dialog)) != 0) return;

        if (state.IsToggled(Settings.Options, ref _prevInputState)) {
            if (PanelManager.CurrentPanelIs(OptionsView.Panel)) {
                PanelManager.ClosePanel();
            } else {
                PanelManager.Enqueue(OptionsView.Panel);
            }
        }
    }

    /// <summary>
    /// Handle any inputs that aren't blocked by anything.
    /// </summary>
    /// <param name="state">Mouse and Keyboard state.</param>
    private static void HandleUnblockedInputs(StateContainer state) {
        if (state.KeyboardState.IsKeyUp(Keys.LeftShift) && HasScrollChanged(state, out var zoomDelta)) {
            Minimap.OnZoom.Dispatch(zoomDelta);
        }

        if (state.KeyboardState.IsKeyDown(Keys.LeftShift) && HasScrollChanged(state, out var delta)) {
            Settings.CameraZoom = Math.Clamp(Settings.CameraZoom += Settings.ScaleFactor * delta, Settings.MinCameraZoom, Settings.MaxCameraZoom);
            Logger.Info($"Camera zoom: {Settings.CameraZoom.Value}");
            Camera.SetZoom(Settings.CameraZoom);
        }
    }

    private static bool HasScrollChanged(StateContainer state, out int delta) {
        var val = state.MouseState.ScrollWheelValue - _prevInputState.MouseState.ScrollWheelValue;
        delta = val switch {
            < 0 => -1,
            > 0 => 1,
            _ => 0
        };

        return delta != 0;
    }
}