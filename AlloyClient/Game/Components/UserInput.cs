using System;
using AlloyClient.Game.Components.Hud;
using AlloyClient.Game.Components.Hud.Chat;
using AlloyClient.Game.Components.Hud.Panels;
using AlloyClient.Game.Components.Options;
using AlloyClient.Networking;
using AlloyClient.Networking.Packets.Outgoing;
using Alloy.UiLib.Core;
using Alloy.Common;
using AlloyClient.Display;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace AlloyClient.Game.Components;

public sealed class UserInput : Sprite {

    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(UserInput));

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
        AddEventListener(Event.AddedToStage, AddedToStage);
        AddEventListener(Event.RemovedFromStage, RemovedFromStage);
    }

    private void AddedToStage() {
        Stage.AddEventListener(KeyboardEvent.KeyDown, OnKeyDown);
        Stage.AddEventListener(KeyboardEvent.KeyUp, OnKeyUp);
        
        Stage.AddEventListener(MouseEvent.LeftDown,OnLeftDown);
        Stage.AddEventListener(MouseEvent.LeftUp, OnLeftUp);
        Stage.AddEventListener(MouseEvent.ScrollVertical, OnScroll);
        Stage.AddEventListener(MouseEvent.MiddleClick, OnMiddleClick);
        Stage.AddEventListener(MouseEvent.MouseMove, OnMouseMove);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }
    
    private void RemovedFromStage() {
        Stage.RemoveEventListener(KeyboardEvent.KeyDown, OnKeyDown);
        Stage.RemoveEventListener(KeyboardEvent.KeyUp, OnKeyUp);
        
        Stage.RemoveEventListener(MouseEvent.LeftDown,OnLeftDown);
        Stage.RemoveEventListener(MouseEvent.LeftUp, OnLeftUp);
        Stage.RemoveEventListener(MouseEvent.ScrollVertical, OnScroll);
        Stage.RemoveEventListener(MouseEvent.MiddleClick, OnMiddleClick);
        Stage.RemoveEventListener(MouseEvent.MouseMove, OnMouseMove);
        RemoveEventListener(Event.EnterFrame, OnFrameEnter);
    }
    
    public static void SetWindowFocus(bool focus) => _windowFocus = focus;

    public static void SetManualFocus(bool focus) => _manualFocus = focus;

    private static bool IsInputDisabled() => !(_windowFocus && _manualFocus);

    private void OnLeftDown(MouseEvent args) {
        if (args.Coords.X > Settings.ScreenWidth - HudView.Width * Stage.ScreenScale.X) return;
        _mouseDown = true;
    }

    private void OnLeftUp() => _mouseDown = false;
    
    private void OnMouseMove(MouseEvent args) => _mousePosition = new Vector2(args.Coords.X, args.Coords.Y);

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
        Map.LocalPlayer?.SetRelativeMovement(0, 0, 0);
    }

    private void OnFrameEnter() {
        if (IsInputDisabled() || !(_mouseDown || _autoFire) || Map.LocalPlayer == null)
            return;
        
        var pos = Camera.ScreenToWorld(_mousePosition);

        var dX = pos.X - Map.LocalPlayer.Position.X;
        var dY = pos.Y - Map.LocalPlayer.Position.Y;
        var angle = MathF.Atan2(dY, dX);
        
        Map.LocalPlayer.Shoot(angle, Stage.GameTime);
    }

    private void SetPlayerMovement() {
        if (Map.LocalPlayer == null) return;
        
        if (IsInputDisabled()) {
            Map.LocalPlayer.SetRelativeMovement(0, 0, 0);
            return;
        }
        
        Map.LocalPlayer.SetRelativeMovement(_rotateRight - _rotateLeft, _moveRight - _moveLeft, _moveDown - _moveUp);
    }

    private void OnScroll(MouseEvent args) {
        if (IsInputDisabled()) return;
        if (Map.LocalPlayer == null) return;
        
        if (args.ShiftKey) {
            Settings.CameraZoom.Set(Math.Clamp(Settings.CameraZoom + 0.1f * args.Delta, Settings.MinCameraZoom, Settings.MaxCameraZoom));
            Logger.Log(LogLevel.Information, $"Camera zoom: {Settings.CameraZoom.Value}");
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
            case true when Settings.RotateLeft.Equals(key):
                _rotateLeft = 1;
                break;
            case true when Settings.RotateRight.Equals(key):
                _rotateRight = 1;
                break;
            case true when Settings.MoveUp.Equals(key):
                _moveUp = 1;
                break;
            case true when Settings.MoveDown.Equals(key):
                _moveDown = 1;
                break;
            case true when Settings.MoveLeft.Equals(key):
                _moveLeft = 1;
                break;
            case true when Settings.MoveRight.Equals(key):
                _moveRight = 1;
                break;
            case true when Settings.AutoFire.Equals(key):
                _autoFire = !_autoFire;
                break;
            case true when Settings.Special.Equals(key):
                //TODO: abilities
                break;
            case true when Settings.Escape.Equals(key):
                if (Map.Name == "Nexus" || Client.IsReconnecting)
                    break;
                
                Client.QueuePacket(Escape.CreatePacket());
                Client.IsReconnecting = true;
                break;
            case true when Settings.Interact.Equals(key):
                if (Client.IsReconnecting)
                    break;
                Panel.OnInteract.Dispatch();
                break;
            case true when Settings.ResetCameraAngle.Equals(key):
                Settings.CameraAngle.Set(0f);
                break;
            case true when Settings.Options.Equals(key):
                ClearMovement();
                SetManualFocus(false);
                OverlayManager.Set(new OptionsView());
                break;
            // Inventory //
            case true when Settings.InvOne.Equals(key):
                break;
            case true when Settings.InvTwo.Equals(key):
                break;
            case true when Settings.InvThree.Equals(key):
                break;
            case true when Settings.InvFour.Equals(key):
                break;
            case true when Settings.InvFive.Equals(key):
                break;
            case true when Settings.InvSix.Equals(key):
                break;
            case true when Settings.InvSeven.Equals(key):
                break;
            case true when Settings.InvEight.Equals(key):
                break;
            case true when Settings.HealthPotion.Equals(key):
                break;
            case true when Settings.MagicPotion.Equals(key):
                break;
            // Chat //
            case true when Settings.Chat.Equals(key):
                ClearMovement();
                ChatBox.OnChatOpen.Dispatch("");
                break;
            case true when Settings.ChatCommand.Equals(key):
                ClearMovement();
                ChatBox.OnChatOpen.Dispatch("/");
                break;
            case true when Settings.TellKey.Equals(key):
                ClearMovement();
                ChatBox.OnChatOpen.Dispatch("/tell ");
                break;
            case true when Settings.GuildChat.Equals(key):
                ClearMovement();
                ChatBox.OnChatOpen.Dispatch("/g ");
                break;
            case true when Settings.PartyChat.Equals(key):
                ClearMovement();
                ChatBox.OnChatOpen.Dispatch("/p ");
                break;
            case true when Settings.ChatHistoryUp.Equals(key):
                ChatBox.OnChatHistoryUp.Dispatch();
                break;
            case true when Settings.ChatHistoryDown.Equals(key):
                ChatBox.OnChatHistoryDown.Dispatch();
                break;
            
        }
        
        SetPlayerMovement();
    }
    
    private void OnKeyUp(KeyboardEvent args) {
        if (IsInputDisabled() || args.Code == Scancode.Unknown) return;
        if (Map.LocalPlayer == null) return;

        var key = args.Code;

        switch (true) {
            case true when Settings.RotateLeft.Equals(key):
                _rotateLeft = 0;
                break;
            case true when Settings.RotateRight.Equals(key):
                _rotateRight = 0;
                break;
            case true when Settings.MoveUp.Equals(key):
                _moveUp = 0;
                break;
            case true when Settings.MoveDown.Equals(key):
                _moveDown = 0;
                break;
            case true when Settings.MoveLeft.Equals(key):
                _moveLeft = 0;
                break;
            case true when Settings.MoveRight.Equals(key):
                _moveRight = 0;
                break;
            case true when Settings.Special.Equals(key):
                //TODO: abilities
                break;
        }
        
        SetPlayerMovement();
    }
}