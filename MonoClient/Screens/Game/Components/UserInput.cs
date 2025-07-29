using System;
using Common;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Screens.Game.Components.Hud;
using MonoClient.Screens.Game.Components.Hud.Chat;
using MonoClient.Screens.Game.Components.Hud.Panels;
using MonoClient.Screens.Game.Components.Options;
using MonoClient.State;
using MonoClient.UiLib.Core;
using MonoClient.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace MonoClient.Screens.Game.Components;

public sealed class UserInput : Sprite {

    private static Vector2 _mousePosition;
    
    private static bool _windowFocus;

    private static bool _manualFocus = true;

    private bool _mouseDown;

    private bool _autoFire;

    private int _rotateRight;
    private int _rotateLeft;

    private int _moveRight;
    private int _moveLeft;
    private int _moveDown;
    private int _moveUp;

    public UserInput() {
        Stage.AddEventListener(KeyboardEvent.KeyDown, OnKeyDown);
        Stage.AddEventListener(KeyboardEvent.KeyUp, OnKeyUp);
        
        Stage.AddEventListener(MouseEvent.LeftDown,OnLeftDown);
        Stage.AddEventListener(MouseEvent.LeftUp, OnLeftUp);
        Stage.AddEventListener(MouseEvent.ScrollVertical, OnScroll);
        Stage.AddEventListener(MouseEvent.MiddleClick, OnMiddleClick);
    }
    
    public static void SetMousePosition(Vector2 pos) => _mousePosition = pos;
    
    public static void SetWindowFocus(bool focus) => _windowFocus = focus;

    public static void SetManualFocus(bool focus) => _manualFocus = focus;

    private static bool IsInputDisabled() => !(_windowFocus && _manualFocus);

    private void OnLeftDown() => _mouseDown = true;
    
    private void OnLeftUp() => _mouseDown = false;

    public void ClearInput() {
        ClearMovement();
        _autoFire = false;
        _mouseDown = false;
    }
    
    public void ClearMovement() {
        _rotateLeft = 0;
        _rotateRight = 0;
        _moveUp = 0;
        _moveDown = 0;
        _moveLeft = 0;
        _moveRight = 0;
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (IsInputDisabled() || !(_mouseDown || _autoFire))
            return;
        
        Map.LocalPlayer.IsShooting = true;
        
        var pos = Camera.ScreenToWorld(_mousePosition);

        var dX = pos.X - Map.LocalPlayer.Position.X;
        var dY = pos.Y - Map.LocalPlayer.Position.Y;

        Map.LocalPlayer.AttackAngle = MathF.Atan2(dY, dX);
        Map.LocalPlayer.Shoot(0f);//TODO: ???
    }

    private void SetPlayerMovement() {
        if (IsInputDisabled()) {
            Map.LocalPlayer.SetRelativeMovement(0, 0, 0);
            return;
        }
        
        Map.LocalPlayer.SetRelativeMovement(_rotateRight - _rotateLeft, _moveRight - _moveLeft, _moveDown - _moveUp);
    }

    private void OnScroll(MouseEvent args) {
        if (IsInputDisabled())
            return;
        if (Map.LocalPlayer == null)
            return;
        
        if (args.ShiftKey) {
            Settings.CameraZoom = Math.Clamp(Settings.CameraZoom += Settings.ScaleFactor * args.Delta, Settings.MinCameraZoom, Settings.MaxCameraZoom);
            Logger.Info($"Camera zoom: {Settings.CameraZoom.Value}");
            Camera.SetZoom(Settings.CameraZoom);
        } else {
            Minimap.OnZoom.Dispatch((int)args.Delta);
        }
    }

    private void OnMiddleClick(MouseEvent args) {
        if (IsInputDisabled())
            return;
    }

    private void OnKeyDown(KeyboardEvent args) {
        if (IsInputDisabled() || args.Code == Scancode.Unknown)
            return;
        if (Map.LocalPlayer == null)
            return;
        
        var key = args.Code;

        switch (true) {
            case true when Settings.RotateLeft.CheckValue(key):
                _rotateLeft = 1;
                break;
            case true when Settings.RotateRight.CheckValue(key):
                _rotateRight = 1;
                break;
            case true when Settings.MoveUp.CheckValue(key):
                _moveUp = 1;
                break;
            case true when Settings.MoveDown.CheckValue(key):
                _moveDown = 1;
                break;
            case true when Settings.MoveLeft.CheckValue(key):
                _moveLeft = 1;
                break;
            case true when Settings.MoveRight.CheckValue(key):
                _moveRight = 1;
                break;
            case true when Settings.AutoFire.CheckValue(key):
                _autoFire = !_autoFire;
                break;
            case true when Settings.Special.CheckValue(key):
                //TODO: abilities
                break;
            case true when Settings.Escape.CheckValue(key):
                if (Map.Name == "Nexus" || Client.IsReconnecting)
                    break;
                
                Client.QueuePacket(Escape.CreatePacket());
                Client.IsReconnecting = true;
                break;
            case true when Settings.Interact.CheckValue(key):
                if (Client.IsReconnecting)
                    break;
                Panel.OnInteract.Dispatch();
                break;
            case true when Settings.ResetCameraAngle.CheckValue(key):
                Settings.CameraAngle = 0;
                break;
            case true when Settings.Options.CheckValue(key):
                ClearInput();
                OptionsView.Toggle();
                break;
            // Inventory //
            case true when Settings.InvOne.CheckValue(key):
                break;
            case true when Settings.InvTwo.CheckValue(key):
                break;
            case true when Settings.InvThree.CheckValue(key):
                break;
            case true when Settings.InvFour.CheckValue(key):
                break;
            case true when Settings.InvFive.CheckValue(key):
                break;
            case true when Settings.InvSix.CheckValue(key):
                break;
            case true when Settings.InvSeven.CheckValue(key):
                break;
            case true when Settings.InvEight.CheckValue(key):
                break;
            case true when Settings.HealthPotion.CheckValue(key):
                break;
            case true when Settings.MagicPotion.CheckValue(key):
                break;
            // Chat //
            case true when Settings.Chat.CheckValue(key):
                ChatView.OnChatKey.Dispatch();
                break;
            case true when Settings.ChatCommand.CheckValue(key):
                ChatView.OnChatOpen.Dispatch("/");
                break;
            case true when Settings.TellKey.CheckValue(key):
                ChatView.OnChatOpen.Dispatch("/tell ");
                break;
            case true when Settings.GuildChat.CheckValue(key):
                ChatView.OnChatOpen.Dispatch("/g ");
                break;
            case true when Settings.PartyChat.CheckValue(key):
                ChatView.OnChatOpen.Dispatch("/p ");
                break;
            case true when Settings.ChatHistoryUp.CheckValue(key):
                ChatView.OnChatHistoryUp.Dispatch();
                break;
            case true when Settings.ChatHistoryDown.CheckValue(key):
                ChatView.OnChatHistoryDown.Dispatch();
                break;
            
        }
        
        SetPlayerMovement();
    }
    
    private void OnKeyUp(KeyboardEvent args) {
        if (IsInputDisabled() || args.Code == Scancode.Unknown)
            return;

        var key = args.Code;

        switch (true) {
            case true when Settings.RotateLeft.CheckValue(key):
                _rotateLeft = 0;
                break;
            case true when Settings.RotateRight.CheckValue(key):
                _rotateRight = 0;
                break;
            case true when Settings.MoveUp.CheckValue(key):
                _moveUp = 0;
                break;
            case true when Settings.MoveDown.CheckValue(key):
                _moveDown = 0;
                break;
            case true when Settings.MoveLeft.CheckValue(key):
                _moveLeft = 0;
                break;
            case true when Settings.MoveRight.CheckValue(key):
                _moveRight = 0;
                break;
            case true when Settings.Special.CheckValue(key):
                //TODO: abilities
                break;
        }
        
        SetPlayerMovement();
    }
}