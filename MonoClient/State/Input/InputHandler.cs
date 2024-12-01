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
using MonoClient.Utils;

namespace MonoClient.State.Input;

public static class InputHandler {
    private static bool _autoFire;
    private static bool _enableInput = true;

    private static StateContainer _prevInputState;
    private static StateContainer _currInputState;

    public static bool Reconnecting;
    public static bool Moving;
    public static bool InOptionPanel = false;

    public static readonly SingleSignal<bool> OnChatKey = new();
    public static readonly SingleSignal OnTellKey = new();
    public static readonly SingleSignal OnGuildChatKey = new();
    public static readonly SingleSignal OnPartyChatKey = new();
    public static readonly SingleSignal OnChatHistoryUp = new();
    public static readonly SingleSignal OnChatHistoryDown = new();

    public static void SetPlayerInput(bool active) => _enableInput = active;
    
    public static void Update(double time, double dt) {
        var state = new StateContainer {
            KeyboardState = Keyboard.GetState(),
            MouseState = Mouse.GetState()
        };

        _prevInputState = _currInputState;
        _currInputState = state;

        if (Map.LocalPlayer != null) {
            if (_enableInput) {
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
                
                if (state.IsPressed(Settings.Options)) {
                    if (InOptionPanel) return;
                    InOptionPanel = true;
                    _enableInput = false;
                    PanelManager.Enqueue(new OptionsView());
                }

                Map.LocalPlayer.Focused = state.IsPressed(Settings.Focus);
            } else {
                Map.LocalPlayer.SetRelativeMovement(0, 0, 0);
            }

            if (state.IsToggled(Settings.Chat, ref _prevInputState)) {
                _enableInput = !_enableInput;
                OnChatKey.Dispatch(!_enableInput);
            }

            if (state.IsToggled(Settings.TellKey, ref _prevInputState) && !ChatView.IsTyping) {
                _enableInput = false;
                OnTellKey.Dispatch();
            }
            
            if (state.IsToggled(Settings.GuildChat, ref _prevInputState) && !ChatView.IsTyping) {
                _enableInput = false;
                OnGuildChatKey.Dispatch();
            }
            
            if (state.IsToggled(Settings.PartyChat, ref _prevInputState) && !ChatView.IsTyping) {
                _enableInput = false;
                OnPartyChatKey.Dispatch();
            }
            
            if (state.IsToggled(Settings.ChatHistoryUp, ref _prevInputState)) {
                OnChatHistoryUp.Dispatch();
            }
            
            if (state.IsToggled(Settings.ChatHistoryDown, ref _prevInputState)) {
                OnChatHistoryDown.Dispatch();
            }
            
            if (state.KeyboardState.IsKeyDown(Keys.LeftShift)) {
                var scrollDelta = state.MouseState.ScrollWheelValue - _prevInputState.MouseState.ScrollWheelValue;

                switch (scrollDelta) {
                    case < 0:
                        Settings.CameraZoom -= Settings.ScaleFactor;
                        if (Settings.CameraZoom < Settings.MinCameraZoom) {
                            Settings.CameraZoom = Settings.MinCameraZoom;
                        }
                    
                        Logger.Info($"Camera zoom: {Settings.CameraZoom.Value}");
                        Camera.SetZoom(Settings.CameraZoom);
                        break;
                    case > 0:
                        Settings.CameraZoom += Settings.ScaleFactor;
                        if (Settings.CameraZoom > Settings.MaxCameraZoom) {
                            Settings.CameraZoom = Settings.MaxCameraZoom;
                        }
                    
                        Logger.Info($"Camera zoom: {Settings.CameraZoom.Value}");
                        Camera.SetZoom(Settings.CameraZoom);
                        break;
                }
            }
            else {
                var scrollDelta = state.MouseState.ScrollWheelValue - _prevInputState.MouseState.ScrollWheelValue;

                if (scrollDelta != 0) {
                    Minimap.OnZoom.Dispatch(scrollDelta > 0 ? 1 : -1);
                }
            }
        }
    }
}