using System.Collections.Generic;
using Common;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.State;
using MonoClient.Ui.Chat;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Signals;

namespace MonoClient.Screens.Game.Components.Hud.Chat;

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
    private TextInput _chatBox;
    private bool _inFocus;

    private string _recentTeller = "";// string.Empty;

    public ChatView() {
        Width = MaxWidth;
        Height = Settings.DefaultScreenHeight;

        _chatBox = new TextInput(new InputConfig {
            FontSize = 18,
            FontType = FontType.Bold,
            OutlineThickness = 3,
            ClickToActivate = true,
            Width = Width,
            OnFocus = OnChatFocus,
            OnUnfocus = OnChatUnFocus
        });
        AddChild(_chatBox);

        _chatContainer = new ChatContainer(new ContainerConfig {
            EnableClip = true,
            Width = Width,
            Height = Height - _chatBox.Height,
        });
        AddChild(_chatContainer);

        _chatBox.Y = _chatContainer.Height;
        _chatBox.Visible = false;

        AddEventListener(Event.AddedToStage, AddHandlers);
        AddEventListener(Event.RemovedToStage, RemoveHandlers);
    }

    private void AddHandlers() {
        OnChatKey.Add(HandleChatKey);
        OnChatOpen.Add(HandleChatOpen);
        OnChatHistoryUp.Add(_chatContainer.PageUp);
        OnChatHistoryDown.Add(_chatContainer.PageDown);
    }

    private void RemoveHandlers() {
        OnChatKey.Remove(HandleChatKey);
        OnChatOpen.Remove(HandleChatOpen);
        OnChatHistoryUp.Remove(_chatContainer.PageUp);
        OnChatHistoryDown.Remove(_chatContainer.PageDown);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (ClearChatRequested) {
            _chatContainer.Clear();
            ClearChatRequested = false;
        }

        while (ChatLineQueue.TryDequeue(out var chatLineData)) {
            _chatContainer.AddChatLine((int) gameTime.TotalMs, chatLineData);
        }
    }

    private void OnChatFocus() {
        UserInput.SetManualFocus(false);
        Map.GameSprite.UserInput.ClearMovement();
        _inFocus = true;
    }

    private void OnChatUnFocus() {
        UserInput.SetManualFocus(true);
        _inFocus = false;
    }

    private void HandleChatKey() {
        if (_inFocus) {
            var msg = _chatBox.Text;
            if (!string.IsNullOrEmpty(msg)) {
                var text = PlayerText.CreatePacket();
                text.Text = msg;
                Client.QueuePacket(text);
                
                if(msg[0] != '/')
                    ChatLayer.QueueSpeech(new SpeechData {Owner = Map.LocalPlayer, Text = _chatBox.Text});
            }

            _chatBox.UnFocus(true);
            _chatBox.Visible = false;
        } else {
            OpenTextInput(_chatBox.Text);
        }
    }

    private void HandleChatOpen(string text) {
        if (_chatBox.Text != "") return;
        
        if (text == "/tell " && !string.IsNullOrEmpty(_recentTeller))
            text = $"/tell {_recentTeller} ";
        
        OpenTextInput(text);
    }

    private void OpenTextInput(string defaultText) {
        if (!_inFocus) {
            _chatBox.Focus();
            _chatBox.Visible = true;
        }

        if (!string.IsNullOrEmpty(defaultText)) {
            _chatBox.InsertText(defaultText);
        }
    }
}