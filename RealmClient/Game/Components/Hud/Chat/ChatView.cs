using System.Collections.Generic;
using Common;
using RealmClient.Networking;
using RealmClient.Networking.Packets.Outgoing;
using RealmClient.State;
using RealmClient.Ui.Chat;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Signals;

namespace RealmClient.Game.Components.Hud.Chat;

// TODO:
// Scroll up/down chat history

public class ChatView : Sprite {

    public static readonly Signal OnChatKey = new();
    public static readonly Signal<string> OnChatOpen = new();
    public static readonly Signal OnChatHistoryUp = new();
    public static readonly Signal OnChatHistoryDown = new();
    
    public static int MaxWidth { get; private set; } = Settings.DefaultScreenWidth / 2;

    private static readonly Queue<ChatLineData> ChatLineQueue = new();
    public static void QueueChatLine(ChatLineData data) => ChatLineQueue.Enqueue(data);

    private static bool ClearChatRequested;
    public static void ClearChat() => ClearChatRequested = true;

    private ChatContainer _chatContainer;
    private readonly TextInput _chatInput;
    private bool _inFocus;

    private string _recentTeller = "";// string.Empty;

    public ChatView() {
        SetAnchor(UiAnchor.LeftBottom);
        //Width = MaxWidth;
        //Height = Settings.DefaultScreenHeight;

        

        _chatContainer = new ChatContainer(new ContainerConfig {
            EnableClip = true,
            Width = MaxWidth,
            Height = 400,
        });
        AddChild(_chatContainer);
        
        _chatInput = new TextInput(new InputConfig {
            FontSize = 18,
            FontType = FontType.Bold,
            OutlineThickness = 3,
            ClickToActivate = true,
            Width = MaxWidth,
            OnFocus = OnChatFocus,
            OnUnfocus = OnChatUnFocus
        });
        AddChild(_chatInput);

        _chatInput.Y = _chatContainer.Height;
        _chatInput.Visible = false;

        AddEventListener(Event.AddedToStage, AddHandlers);
        AddEventListener(Event.RemovedFromStage, RemoveHandlers);
    }

    private void AddHandlers() {
        OnChatKey.Add(HandleChatKey);
        OnChatOpen.Add(HandleChatOpen);
        OnChatHistoryUp.Add(_chatContainer.PageUp);
        OnChatHistoryDown.Add(_chatContainer.PageDown);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void RemoveHandlers() {
        OnChatKey.Remove(HandleChatKey);
        OnChatOpen.Remove(HandleChatOpen);
        OnChatHistoryUp.Remove(_chatContainer.PageUp);
        OnChatHistoryDown.Remove(_chatContainer.PageDown);
        RemoveEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void OnFrameEnter() {
        if (ClearChatRequested) {
            _chatContainer.Clear();
            ClearChatRequested = false;
        }

        while (ChatLineQueue.TryDequeue(out var chatLineData)) {
            _chatContainer.AddChatLine((int) Stage.GameTime.TotalMs, chatLineData);
        }
    }

    private void OnChatFocus() {
        UserInput.SetManualFocus(false);
        Map.GameSprite.UserInput.ClearMovement();
        _inFocus = true;
        Stage.AddEventListener(KeyboardEvent.KeyDown, OnKeyDown);
    }

    private void OnChatUnFocus() {
        UserInput.SetManualFocus(true);
        _inFocus = false;
        Stage.RemoveEventListener(KeyboardEvent.KeyDown, OnKeyDown);
    }

    private void OnKeyDown(KeyboardEvent args) {
        if (args.Code == Settings.Chat.Key) {
            HandleChatKey();
        }
    }

    private void HandleChatKey() {
        if (_inFocus) {
            var msg = _chatInput.Text;
            if (!string.IsNullOrEmpty(msg)) {
                var text = PlayerText.CreatePacket();
                text.Text = msg;
                Client.QueuePacket(text);
                
                if(msg[0] != '/')
                    ChatLayer.QueueSpeech(new SpeechData {Owner = Map.LocalPlayer, Text = _chatInput.Text});
            }

            _chatInput.UnFocus(true);
            _chatInput.Visible = false;
        } else {
            OpenTextInput(_chatInput.Text);
        }
    }

    private void HandleChatOpen(string text) {
        if (_chatInput.Text != "") return;
        
        if (text == "/tell " && !string.IsNullOrEmpty(_recentTeller))
            text = $"/tell {_recentTeller} ";
        
        OpenTextInput(text);
    }

    private void OpenTextInput(string defaultText) {
        if (!_inFocus) {
            _chatInput.Focus();
            _chatInput.Visible = true;
        }

        if (!string.IsNullOrEmpty(defaultText)) {
            _chatInput.InsertText(defaultText);
        }
    }
}